using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace mazharenko.AoCAgent.Client;
	
internal class AoCClient : IAoCClient
{
	private readonly int year;
	private readonly HttpClient httpClient;

	public AoCClient(int year, string sessionToken)
	{
		this.year = year;
		httpClient = new HttpClient
		{
			BaseAddress = new Uri("https://adventofcode.com")
		};
		httpClient.DefaultRequestHeaders.Add("cookie", "session=" + sessionToken);
		httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"aoc-agent/{ThisAssembly.Info.Version} (+via github.com/mazharenko/aoc-agent by mazharenko.a@gmail.com)");
	}
	
	public async Task<string> LoadInput(Day day)
	{
		var response = await httpClient.GetAsync($"/{year}/day/{day.Num}/input");
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadAsStringAsync();
	}

	public async Task<SubmissionResult> SubmitAnswer(Day day, Part part, string answer)
	{
		var form = new FormUrlEncodedContent(
			new Dictionary<string, string>
			{
				["level"] = part.Num.ToString(),
				["answer"] = answer
			}
		);
		var response = await httpClient.PostAsync($"/{year}/day/{day.Num}/answer", form);
		response.EnsureSuccessStatusCode();
		var content = await response.Content.ReadAsStringAsync();
		if (content.Contains("That's the right answer!")) return new SubmissionResult.Correct();
		if (content.Contains("too high")) return new SubmissionResult.TooHigh();
		if (content.Contains("too low")) return new SubmissionResult.TooLow();
		if (content.Contains("That's not the right answer")) return new SubmissionResult.Incorrect();
		if (content.Contains("You don't seem to be solving the right level."))
		{
			if ((await GetDayResults()).TryGetValue((day, part), out var dayResult))
				return dayResult ? new SubmissionResult.Correct() : new SubmissionResult.Incorrect();
			throw new InvalidOperationException("Could not interpret the submission result");
		}
		var timeLeftRegex = new Regex(@"You have ((?<min>\d+)m )?((?<sec>\d+)s )?left");
		var timeLeftMatch = timeLeftRegex.Match(content);
		if (timeLeftMatch.Success)
		{
			var min = timeLeftMatch.Groups["min"].Success
				? int.Parse(timeLeftMatch.Groups["min"].Value)
				: 0;
			var sec = timeLeftMatch.Groups["sec"].Success
				? int.Parse(timeLeftMatch.Groups["sec"].Value)
				: 0;
			return new SubmissionResult.TooRecently(new TimeSpan(0, min, sec));
		}
			
		throw new InvalidOperationException("Could not interpret the submission result");
	}

	private static readonly Regex DayRegex = new("""
	                                             aria-label="Day (?<day>\d+)(, (?<stars>((one star)|(two stars))?))?"
	                                             """);
	public async Task<IImmutableDictionary<(Day, Part), bool>> GetDayResults()
	{
		var dic = ImmutableDictionary.CreateBuilder<(Day, Part), bool>();
		await foreach (var (day, part, solved) in _GetDayResults()) 
			dic.Add((day, part), solved);
		return dic.ToImmutable();
	}
	private async IAsyncEnumerable<(Day, Part, bool)> _GetDayResults()
	{
		var response = await httpClient.GetStringAsync($"/{year}");
		var statParsed = DayRegex.Matches(response);
		
		foreach (Match match in statParsed)
		{
			var day = Day.Create(int.Parse(match.Groups["day"].Value));
			switch (match.Groups["stars"].Value)
			{
				case "":
					yield return (day, Part._1, false);
					yield return (day, Part._2, false);
					break;
				case "one star":
					yield return (day, Part._1, true);
					yield return (day, Part._2, false);
					break;
				case "two stars":
					yield return (day, Part._1, true);
					yield return (day, Part._2, true);
					break;
				default:
					throw new InvalidOperationException($"Could not interpret day results: {match.Value}");
			}
		}
	}

	public void Dispose()
	{
		httpClient.Dispose();
	}
}
