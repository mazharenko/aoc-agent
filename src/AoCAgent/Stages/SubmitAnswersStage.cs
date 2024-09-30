using System.Diagnostics;
using mazharenko.AoCAgent.Client;
using mazharenko.AoCAgent.Misc;
using Spectre.Console;

namespace mazharenko.AoCAgent.Stages;

internal class SubmitAnswersStage(RunnerContext runnerContext)
{
	public async Task<SubmitAnswersResult> CalculateAndSubmit(List<(int day, RunnerPart, CheckExamplesResult)> exampleResults)
	{
		var atLeastOneCorrectAnswer = false;
		var passedDayParts = exampleResults
			.Choose(x =>
			{
				var (day, part, result) = x;
				return result is CheckExamplesResult.AllCorrect or CheckExamplesResult.SkipNoExamples
					? (day, part).ToNullable()
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
			string.Join(", ", passedDayParts.Select(x => $"[yellow bold]{x.day:00}/{x.part.Num}[/]")));

		foreach (var (day, part) in passedDayParts)
		{
			var (answer, calculationTime) = await new Status(runnerContext.Console)
				.StartAsync($"Obtaining input for day {day:00}", async ctx =>
				{
					var obtained = await runnerContext.AoCClient.LoadInput(Day.Create(day));
					var sw = Stopwatch.StartNew();
					runnerContext.Console.MarkupLine($"[[{day:00}/{part.Num}]] :check_mark: Input obtained");
					ctx.Status($"Calculating answer for {day:00}/{part.Num}");
					return (part.Part.SolveString(obtained), sw.Elapsed);
				});

			if (part.Part.Settings.ManualInterpretation)
			{
				AnsiConsole.MarkupLine(
					$"[[{day:00}/{part.Num}]] :check_mark: Answer calculated in {calculationTime.ToHumanReadable()}");
				AnsiConsole.WriteLine(answer);
				var answerToSubmit = AnsiConsole.Prompt(new TextPrompt<string>("[yellow bold]Interpret the answer manually[/]"));
				atLeastOneCorrectAnswer |= await SubmitAnswer(runnerContext.AoCClient, day, part, answerToSubmit);
			}
			else
			{
				runnerContext.Console.MarkupLine(
					$"[[{day:00}/{part.Num}]] :check_mark: Answer '{answer}' calculated in {calculationTime.ToHumanReadable()}");

				atLeastOneCorrectAnswer |= await SubmitAnswer(runnerContext.AoCClient, day, part, answer);
			}
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

	private async Task<bool> SubmitAnswer(IAoCClient client, int day, RunnerPart part, string answer)
	{
		return await new Status(runnerContext.Console).StartAsync($"Submitting answer '{answer}'", async ctx =>
		{
			while (true)
			{
				var submissionResult = await client.SubmitAnswer(Day.Create(day), Part.Create(part.Num), answer);
				switch (submissionResult)
				{
					case SubmissionResult.Correct:
						runnerContext.Console.MarkupLine($"[[{day:00}/{part.Num}]]");
						runnerContext.Console.Write(Renderables.Correct());
						return true;
					case SubmissionResult.Incorrect:
						runnerContext.Console.MarkupLine($"[[{day:00}/{part.Num}]]");
						runnerContext.Console.Write(Renderables.Incorrect("wrong"));
						return false;
					case SubmissionResult.TooHigh:
						runnerContext.Console.MarkupLine($"[[{day:00}/{part.Num}]]");
						runnerContext.Console.Write(Renderables.Incorrect("too high"));
						return false;
					case SubmissionResult.TooLow:
						runnerContext.Console.MarkupLine($"[[{day:00}/{part.Num}]]");
						runnerContext.Console.Write(Renderables.Incorrect("too low"));
						return false;
					case SubmissionResult.TooRecently(var toWait):
						runnerContext.Console.MarkupLine(
							$"[[{day:00}/{part.Num}]] :timer_clock: Answer given too recently. Need to wait {toWait.ToHumanReadable()}");
						var timeoutStopwatch = Stopwatch.StartNew();
						var leftToWait = toWait - timeoutStopwatch.Elapsed;
						while (leftToWait >= TimeSpan.Zero)
						{
							ctx.Status(
								$"Waiting [yellow bold]{leftToWait.ToHumanReadable()}[/] more before another attempt");
							await Task.Delay(leftToWait.TotalMilliseconds > 1000
								? 1000
								: (int)leftToWait.TotalMilliseconds);
							leftToWait = toWait - timeoutStopwatch.Elapsed;
						}

						continue;
					default:
						throw new ArgumentOutOfRangeException(nameof(submissionResult));
				}
			}
		});
	}
}