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

internal interface IAoCClient : IDisposable
{
	Task<string> LoadInput(Day day);
	Task<SubmissionResult> SubmitAnswer(Day day, Part part, string answer);
	Task<IImmutableDictionary<(Day, Part), bool>> GetDayResults();
}