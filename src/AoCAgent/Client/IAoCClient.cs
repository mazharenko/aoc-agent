using System.Collections.Immutable;

namespace mazharenko.AoCAgent.Client;

internal abstract record SubmissionResult
{
	private SubmissionResult(){}
	public record Correct : SubmissionResult;
	public record Incorrect : SubmissionResult;
	public record TooLow : SubmissionResult;
	public record TooHigh : SubmissionResult;
	public record TooRecently(TimeSpan LeftToWait) : SubmissionResult;
}

internal class Stats : Dictionary<(Day, Part), bool>
{
	public Stats()
	{
	}
	public Stats(IDictionary<(Day, Part), bool> dictionary) : base(dictionary)
	{
	}

	public bool IsSolved(Day day, Part part)
	{
		return this.GetValueOrDefault((Day.Create(day.Num), part), false);
	}
	
	public int Stars => this.Sum(x => x.Value ? 1 : 0);

	public bool AllComplete()
	{
		return Stars == 50;
	}
}

internal interface IAoCClient 
{
	Task<string> LoadInput(Day day);
	Task<SubmissionResult> SubmitAnswer(Day day, Part part, string answer);
	Task<Stats> GetDayResults();
	Task AcquireStar50();
}