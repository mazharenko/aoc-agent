using mazharenko.AoCAgent.Base;

namespace mazharenko.AoCAgent.Stages;

internal abstract record CheckExamplesResult
{
	private CheckExamplesResult()
	{
	}

	public record AllCorrect : CheckExamplesResult;

	public record NotImplemented : CheckExamplesResult;

	public record NoExamples : CheckExamplesResult;

	public record SkipNoExamples : CheckExamplesResult;

	public record Failed(IList<(NamedExample example, string? actual, Exception? exception)> FailedExamples) : CheckExamplesResult;
}