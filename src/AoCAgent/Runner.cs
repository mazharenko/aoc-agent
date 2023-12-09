using System.Collections.Immutable;
using System.Diagnostics;
using JetBrains.Annotations;
using mazharenko.AoCAgent.Base;
using mazharenko.AoCAgent.Client;
using mazharenko.AoCAgent.Misc;
using Spectre.Console;

namespace mazharenko.AoCAgent;

[PublicAPI]
public class Runner
{
	private static string GetSessionKey()
	{
		const string sessionCookieFileName = "SESSION.COOKIE";

		var fileExists = File.Exists(sessionCookieFileName);
		var existingSessionValue =
			fileExists ? File.ReadAllText(sessionCookieFileName) : "";

		switch (fileExists, existingSessionValue, DotnetWatch.IsUnderWatch())
		{
			case (false, _, false):
			case (true, "", false):
				var newSessionValue = AnsiConsole.Prompt(new TextPrompt<string>("Session key was not found. Please provide its value:").Secret());
				File.WriteAllText(sessionCookieFileName, newSessionValue);
				return newSessionValue;
			case (false, _, true):
				File.WriteAllText(sessionCookieFileName, "");
				throw new Exception("Session key was not found. Empty file is created");
			case (true, "", true):
				throw new Exception("Session key value was not found");
			case (true, var foundValue, _):
				return foundValue.Trim();
		}
	}

	public async Task Run(YearBase year)
	{
		var sessionKey = GetSessionKey();
		using var client = new AoCCachingClient(year.Year, new AoCClient(year.Year, sessionKey));

		var currentStats =
			await AnsiConsole.Live(Renderables.Splash(year.Year))
				.StartAsync(async ctx =>
				{
					ctx.Refresh();
					// ReSharper disable once AccessToDisposedClosure
					var stats = await client.GetDayResults();
					var stars = stats.Sum(x => x.Value ? 1 : 0);
					ctx.UpdateTarget(Renderables.Splash(year.Year, stars));
					return stats;
				});

		var allExamplesCorrect = CollectCandidates(year, currentStats, out var failedExamples);
		if (allExamplesCorrect.Count == 0)
		{
			AnsiConsole.MarkupLine(
				"[green bold]There are no days that are not solved yet but implemented and worth trying to calculate and submit answers for.[/]");
			return;
		}

		AnsiConsole.MarkupLine(
			$"Overall [yellow bold]{allExamplesCorrect.Count}[/] days and parts that are not solved but examples were " +
			$"passed for. Worth trying to calculate and submit answers for them: " +
			string.Join(", ", allExamplesCorrect.Select(x => $"[yellow bold]{x.day.Num:00}/{x.part.Num}[/]")));

		var atLeastOneCorrectAnswer = false;
		foreach (var (day, part) in allExamplesCorrect)
		{
			var (answer, calculationTime) = await AnsiConsole.Status()
				.StartAsync($"Obtaining input for day {day.Num:00}", async ctx =>
				{
					var obtained = await client.LoadInput(Day.Create(day.Num));
					var sw = Stopwatch.StartNew();
					AnsiConsole.MarkupLine($"[[{day.Num:00}/{part.Num}]] :check_mark: Input obtained");
					ctx.Status($"Calculating answer for {day.Num:00}/{part.Num}");
					return (part.Part.SolveString(obtained), sw.Elapsed);
				});

			if (part.Part.Settings.ManualInterpretation)
			{
				AnsiConsole.MarkupLine(
					$"[[{day.Num:00}/{part.Num}]] :check_mark: Answer calculated in {calculationTime.ToHumanReadable()}");
				AnsiConsole.WriteLine(answer);
				var answerToSubmit = AnsiConsole.Prompt(new TextPrompt<string>("[yellow bold]Interpret the answer manually[/]"));
				await SubmitAnswer(client, day, part, answerToSubmit);
			}
			else
			{
				AnsiConsole.MarkupLine(
					$"[[{day.Num:00}/{part.Num}]] :check_mark: Answer '{answer}' calculated in {calculationTime.ToHumanReadable()}");

				atLeastOneCorrectAnswer |= await SubmitAnswer(client, day, part, answer);
			}
		}
		
		if (failedExamples.Count > 0)
		{
			AnsiConsole.MarkupLine("[yellow bold]Please revise failed examples[/]");

			var table = new Table();
			table.AddColumn("Day");
			table.AddColumn("Part");
			table.AddColumn("Example");
			table.AddColumn("Expected");
			table.AddColumn("Actual");
			table.SimpleBorder();
			foreach (var (day, part, failed) in failedExamples)
			{
				foreach (var (runnerExample, actual, exception) in failed.FailedExamples)
				{
					table.AddRow(day.Num.ToString("00"), part.Num.ToString(),
						runnerExample.Name,
						runnerExample.Example.ExpectationFormatted,
						$"[red]{Markup.Escape(exception?.Message ?? actual ?? "")}[/]");
				}
			}

			AnsiConsole.Write(table);
		}
		else if (atLeastOneCorrectAnswer)
		{
			var stats = await client.GetDayResults();
			var stars = stats.Sum(x => x.Value ? 1 : 0);
			AnsiConsole.Write(Renderables.Splash(year.Year, stars));
		}

	}

	private static async Task<bool> SubmitAnswer(IAoCClient client, RunnerDay day, RunnerPart part, string answer)
	{
		return await AnsiConsole.Status().StartAsync($"Submitting answer '{answer}'", async ctx =>
		{
			while (true)
			{
				var submissionResult = await client.SubmitAnswer(Day.Create(day.Num), Part.Create(part.Num), answer);
				switch (submissionResult)
				{
					case SubmissionResult.Correct:
						AnsiConsole.MarkupLine($"[[{day.Num:00}/{part.Num}]]");
						AnsiConsole.Write(Renderables.Correct);
						return true;
					case SubmissionResult.Incorrect:
						AnsiConsole.MarkupLine($"[[{day.Num:00}/{part.Num}]]");
						AnsiConsole.Write(Renderables.Incorrect("wrong"));
						return false;
					case SubmissionResult.TooHigh:
						AnsiConsole.MarkupLine($"[[{day.Num:00}/{part.Num}]]");
						AnsiConsole.Write(Renderables.Incorrect("too high"));
						return false;
					case SubmissionResult.TooLow:
						AnsiConsole.MarkupLine($"[[{day.Num:00}/{part.Num}]]");
						AnsiConsole.Write(Renderables.Incorrect("too low"));
						break;
					case SubmissionResult.TooRecently(var leftToWait):
						AnsiConsole.MarkupLine(
							$"[[{day.Num:00}/{part.Num}]] :timer_clock: Answer given too recently. Need to wait {leftToWait.ToHumanReadable()}");
						var timeoutStopwatch = Stopwatch.StartNew();
						while (timeoutStopwatch.Elapsed < leftToWait)
						{
							await Task.Delay(1000);
							ctx.Status(
								$"Waiting [yellow bold]{(leftToWait - timeoutStopwatch.Elapsed).ToHumanReadable()}[/] more before another attempt");
						}
						continue;
					default:
						throw new ArgumentOutOfRangeException(nameof(submissionResult));
				}
			}
		});
	}

	private static List<(RunnerDay day, RunnerPart part)> CollectCandidates(YearBase year, IImmutableDictionary<(Day, Part), bool> currentStats,
		out List<(RunnerDay day, RunnerPart part, ExampleCheckResult.Failed failed)> failedExamples)
	{
		var dayExampleResults =
			AnsiConsole.Status()
				.Start("Checking if there are days that are implemented but not solved yet", ctx =>
				{
					var notSolvedDays =
						year.Days.OrderByDescending(day => day.Num)
							.Select(day =>
							{
								var part1Solved = currentStats.GetValueOrDefault((Day.Create(day.Num), Part._1), false);
								var part2Solved = currentStats.GetValueOrDefault((Day.Create(day.Num), Part._2), false);
								if (!part1Solved)
									return (day, day.Part1);
								if (!part2Solved)
									return (day, day.Part2);

								return ((RunnerDay, RunnerPart)?)null;
							}).Where(x => x.HasValue)
							.Select(x => x!.Value)
							.ToList();

					if (notSolvedDays.Count == 0)
					{
						AnsiConsole.MarkupLine("[green bold]There are no days that are not solved yet[/]");
						return new List<(RunnerDay, RunnerPart, ExampleCheckResult)>();
					}

					AnsiConsole.MarkupLine($"Found [yellow bold]{notSolvedDays.Count}[/] not solved days and parts: " +
					                       string.Join(", ", notSolvedDays.Select(x => $"{x.Item1.Num:00}/{x.Item2.Num}")));

					ctx.Status("Calculating examples");

					var dayExampleResults =
						notSolvedDays.Select(x =>
						{
							var (day, part) = x;
							var result = CheckExamples(part);
							var status = result switch
							{
								ExampleCheckResult.AllCorrect => "[green bold]all correct[/]",
								ExampleCheckResult.SkipNoExamples => "[green bold]no examples[/]",
								ExampleCheckResult.Failed failed => $"[red]failed {failed.FailedExamples.Count} examples[/]",
								ExampleCheckResult.NoExamples => "[grey]no examples[/]",
								ExampleCheckResult.NotImplemented => "[grey]not implemented[/]",
								_ => throw new ArgumentOutOfRangeException()
							};
							AnsiConsole.MarkupLine($"Day {day.Num:00} Part {part.Num} - {status}");
							return (day, part, result);
						}).ToList();

					return dayExampleResults;
				});

		failedExamples = dayExampleResults.Choose(x =>
		{
			var (day, part, result) = x;
			if (result is ExampleCheckResult.Failed failed)
				return (day, part, failed).ToNullable();
			return null;
		}).ToList();

		return dayExampleResults
			.Choose(x =>
			{
				var (day, part, result) = x;
				return result is ExampleCheckResult.AllCorrect or ExampleCheckResult.SkipNoExamples
					? (day, part).ToNullable()
					: null;
			}).ToList();
	}

	private abstract record ExampleCheckResult
	{
		private ExampleCheckResult(){ }
		public record AllCorrect : ExampleCheckResult;
		public record NotImplemented : ExampleCheckResult;
		public record NoExamples : ExampleCheckResult;
		public record SkipNoExamples : ExampleCheckResult;
		public record Failed(IList<(NamedExample, string?, Exception?)> FailedExamples) : ExampleCheckResult;
	}

	private static ExampleCheckResult CheckExamples(RunnerPart part)
	{
		var examples = part.Part.GetExamples().ToList();
		if (examples.Count == 0 && part.Part.Settings.BypassNoExamples)
			return new ExampleCheckResult.SkipNoExamples();
		if (examples.Count == 0)
			return new ExampleCheckResult.NoExamples();
		IList<(NamedExample, string?, Exception?)> failedExamples = new List<(NamedExample, string?, Exception?)>();
		foreach (var example in examples)
		{
			try
			{
				var actual = example.Example.RunFormat(out var actualFormatted);
				if (!Equals(actual, example.Example.Expectation))
					failedExamples.Add((example, actualFormatted, null));
			}
			catch (NotImplementedException)
			{
				return new ExampleCheckResult.NotImplemented();
			}
			catch (Exception e)
			{
				failedExamples.Add((example, null, e));
			}
		}
		
		if (failedExamples.Count == 0)
			return new ExampleCheckResult.AllCorrect();
		return new ExampleCheckResult.Failed(failedExamples);
	}
}