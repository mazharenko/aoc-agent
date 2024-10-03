using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace mazharenko.AoCAgent.Client;
	
internal class AoCClient(int year, string sessionToken, IHttpClientFactory httpClientFactory) : IAoCClient
{
	private HttpClient CreateHttpClient()
	{
		var httpClient = httpClientFactory.CreateClient();
		httpClient.BaseAddress = new Uri("https://adventofcode.com");
		httpClient.DefaultRequestHeaders.Add("cookie", "session=" + sessionToken);
		httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"aoc-agent/{ThisAssembly.Info.Version} (+via github.com/mazharenko/aoc-agent by mazharenko.a@gmail.com)");
		return httpClient;
	}
	
	public async Task<string> LoadInput(DayNum day)
	{
		using var httpClient = CreateHttpClient();
		var response = await httpClient.GetAsync($"/{year}/day/{day.Num}/input");
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadAsStringAsync();
	}

	public async Task AcquireStar50()
	{
		var form = new FormUrlEncodedContent(
			new Dictionary<string, string>
			{
				["level"] = "2",
				["answer"] = "0"
			}
		);
		using var httpClient = CreateHttpClient();
		var response = await httpClient.PostAsync($"/{year}/day/25/answer", form);
		response.EnsureSuccessStatusCode();
		var content = await response.Content.ReadAsStringAsync();
		if (content.Contains("You've finished every puzzle"))
			return;
		if (content.Contains("You don't seem to be solving the right level."))
		{
			if ((await GetDayResults()).IsSolved(DayNum.Create(25), PartNum._2))
				return;
			throw new InvalidOperationException("To acquire star 50, all other puzzles must be solved");
		}
		throw new InvalidOperationException("Could not interpret the submission result");
	}

	public async Task<SubmissionResult> SubmitAnswer(DayNum day, PartNum part, string answer)
	{
		// Congratulations! You've finished every puzzle in Advent of Code 2023
		var form = new FormUrlEncodedContent(
			new Dictionary<string, string>
			{
				["level"] = part.Num.ToString(),
				["answer"] = answer
			}
		);
		using var httpClient = CreateHttpClient();
		var response = await httpClient.PostAsync($"/{year}/day/{day.Num}/answer", form);
		response.EnsureSuccessStatusCode();
		var content = await response.Content.ReadAsStringAsync();
		if (content.Contains("That's the right answer!")) return new SubmissionResult.Correct();
		if (content.Contains("too high")) return new SubmissionResult.TooHigh();
		if (content.Contains("too low")) return new SubmissionResult.TooLow();
		if (content.Contains("That's not the right answer")) return new SubmissionResult.Incorrect();
		if (content.Contains("You don't seem to be solving the right level."))
		{
			if ((await GetDayResults()).IsSolved(day, part))
				return new SubmissionResult.Correct();
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
	public async Task<Stats> GetDayResults()
	{
		var stats = new Stats();
		await foreach (var (day, part) in _GetDayResults()) 
			stats.Solved(day, part);
		return stats;
	}
	
	private async IAsyncEnumerable<(DayNum, PartNum)> _GetDayResults()
	{
		using var httpClient = CreateHttpClient();
		var response = await httpClient.GetStringAsync($"/{year}");
		var statParsed = DayRegex.Matches(response);
		
		foreach (Match match in statParsed)
		{
			var day = DayNum.Create(int.Parse(match.Groups["day"].Value));
			switch (match.Groups["stars"].Value)
			{
				case "":
					break;
				case "one star":
					yield return (day, PartNum._1);
					break;
				case "two stars":
					yield return (day, PartNum._1);
					yield return (day, PartNum._2);
					break;
				default:
					throw new InvalidOperationException($"Could not interpret day results: {match.Value}");
			}
		}
	}
}
