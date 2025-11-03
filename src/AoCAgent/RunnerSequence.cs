using mazharenko.AoCAgent.Stages;

namespace mazharenko.AoCAgent;

internal class RunnerSequence(
	RunnerContext runnerContext,
	StatsStage statsStage,
	CheckExamplesStage checkExamplesStage,
	SubmitAnswersStage submitAnswersStage,
	FailedExamplesStage failedExamplesStage)
{
	public async Task Run()
	{
		var currentStats = await runnerContext.GetCurrentStats();
		statsStage.RenderStats(currentStats);
		if (currentStats.Stars == runnerContext.Year.MaxStars)
			return;
		var exampleResults = checkExamplesStage.CheckExamples(currentStats);
		var submitAnswersResult = await submitAnswersStage.CalculateAndSubmit(exampleResults);
		var failedExamplesResult = failedExamplesStage.ReportFailedExamples(exampleResults);
		if (submitAnswersResult.AtLeastOneCorrectAnswer && !failedExamplesResult.AtLeastOneFailedExample)
			statsStage.RenderStats(await runnerContext.GetCurrentStats());
	}
}