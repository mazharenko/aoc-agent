using mazharenko.AoCAgent.Client;
using mazharenko.AoCAgent.Misc;
using Spectre.Console;

namespace mazharenko.AoCAgent.Stages;

internal class SubmitAnswersStage(RunnerContext runnerContext, SubmitAnswerSubStage submitAnswerSubStage)
{
	public async Task<SubmitAnswersResult> CalculateAndSubmit(List<(RunnerPart, CheckExamplesResult)> exampleResults)
	{
		var atLeastOneCorrectAnswer = false;
		var passedDayParts = exampleResults
			.Choose(x =>
			{
				var (part, result) = x;
				return result is CheckExamplesResult.AllCorrect or CheckExamplesResult.SkipNoExamples
					? part
					: null;
			}).ToList();
		if (passedDayParts.Count == 0)
		{
			runnerContext.Console.MarkupLine(
				"[green bold]There are no days that are not solved yet but implemented and worth trying to calculate and submit answers for.[/]");
			return new SubmitAnswersResult(false);
		}

		runnerContext.Console.MarkupLine(
			$"Overall [yellow bold]{passedDayParts.Count}[/] days and parts that are not solved but examples were " +
			$"passed for. Worth trying to calculate and submit answers for them: " +
			string.Join(", ", passedDayParts.Select(x => $"[yellow bold]{x.Day:00}/{x.PartNum}[/]")));

		foreach (var part in passedDayParts)
		{
			atLeastOneCorrectAnswer |= await submitAnswerSubStage.CalculateAndSubmit(part);
		}

		var newStats = await runnerContext.AoCClient.GetDayResults();
		if (newStats.Stars == 49)
		{
			runnerContext.Console.MarkupLine("[green bold]49 stars have been acquired. Claiming the last one[/]");
			await SubmitStar50(runnerContext.AoCClient);
			atLeastOneCorrectAnswer = true;
		}

		return new SubmitAnswersResult(atLeastOneCorrectAnswer);
	}

	private async Task SubmitStar50(IAoCClient client)
	{
		await new Status(runnerContext.Console).StartAsync("Claiming star 50", async ctx =>
		{
			await client.AcquireStar50();
			runnerContext.Console.MarkupLine("[[25/2]]");
			runnerContext.Console.Write(Renderables.Correct("50!"));
		});
	}
}