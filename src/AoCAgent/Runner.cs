using System.Diagnostics;
using JetBrains.Annotations;
using mazharenko.AoCAgent.Base;
using mazharenko.AoCAgent.Client;
using mazharenko.AoCAgent.Stages;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace mazharenko.AoCAgent;

[PublicAPI]
public class Runner
{
	// todo
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
		var services =
				new ServiceCollection()
					.AddSingleton(year)
					.AddSingleton<IAoCClient>(serviceProvider => 
						new AoCCachingClient(year.Year, new AoCClient(year.Year, GetSessionKey(), serviceProvider.GetRequiredService<IHttpClientFactory>())))
					.AddSingleton<IAnsiConsole>(_ => AnsiConsole.Create(new AnsiConsoleSettings()))
					.AddSingleton<RunnerContext>()
					.AddSingleton<RunnerSequence>()
					.AddSingleton<StatsStage>()
					.AddSingleton<CheckExamplesStage>()
					.AddSingleton<ICheckPartExamplesSubStage, CheckPartExamplesSubStage>()
					.AddSingleton<SubmitAnswerSubStage>()
					.AddSingleton<FailedExamplesStage>()
					.AddSingleton<SubmitAnswersStage>()
					.AddHttpClient()
			;
		await using var serviceProvider = services.BuildServiceProvider();
		await serviceProvider.GetRequiredService<RunnerSequence>().Run();
		return;
/*
		var sessionKey = GetSessionKey();
		using var client = new AoCCachingClient(year.Year, new AoCClient(year.Year, sessionKey));

		var currentStats =
			await AnsiConsole.Live(Renderables.Splash(year.Year))
				.StartAsync(async ctx =>
				{
					ctx.Refresh();
					// ReSharper disable once AccessToDisposedClosure
					var stats = await client.GetDayResults();
					var stars = stats.Stars;
					Thread.Sleep(3000);
					ctx.UpdateTarget(Renderables.Splash(year.Year, stars));
					return stats;
				});

		var atLeastOneCorrectAnswer = false;

		if (currentStats.AllComplete())
			return;

		var allExamplesCorrect = CollectCandidates(year, currentStats, out var failedExamples);
		if (allExamplesCorrect.Count == 0)
		{
			AnsiConsole.MarkupLine(
				"[green bold]There are no days that are not solved yet but implemented and worth trying to calculate and submit answers for.[/]");
		}
		else
		{
			AnsiConsole.MarkupLine(
				$"Overall [yellow bold]{allExamplesCorrect.Count}[/] days and parts that are not solved but examples were " +
				$"passed for. Worth trying to calculate and submit answers for them: " +
				string.Join(", ", allExamplesCorrect.Select(x => $"[yellow bold]{x.day.Num:00}/{x.part.Num}[/]")));

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
					atLeastOneCorrectAnswer |= await SubmitAnswer(client, day, part, answerToSubmit);
				}
				else
				{
					AnsiConsole.MarkupLine(
						$"[[{day.Num:00}/{part.Num}]] :check_mark: Answer '{answer}' calculated in {calculationTime.ToHumanReadable()}");

					atLeastOneCorrectAnswer |= await SubmitAnswer(client, day, part, answer);
				}
			}
		}

		var newStats = await client.GetDayResults();
		if (newStats.Stars == 49)
		{
			AnsiConsole.MarkupLine("[green bold]49 stars have been acquired. Claiming the last one[/]");
			await SubmitStar50(client);
			newStats = await client.GetDayResults();
			atLeastOneCorrectAnswer = true;
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
			AnsiConsole.Write(Renderables.Splash(year.Year, newStats.Stars));
	}

	private static async Task SubmitStar50(IAoCClient client)
	{
		await AnsiConsole.Status().StartAsync("Claiming star 50", async ctx =>
		{
			await client.AcquireStar50();
			AnsiConsole.MarkupLine("[[25/2]]");
			AnsiConsole.Write(Renderables.Correct("50!"));
		});
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
						AnsiConsole.Write(Renderables.Correct());
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
						return false;
					case SubmissionResult.TooRecently(var toWait):
						AnsiConsole.MarkupLine(
							$"[[{day.Num:00}/{part.Num}]] :timer_clock: Answer given too recently. Need to wait {toWait.ToHumanReadable()}");
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
		});*/
	}
/*
	private static List<(RunnerDay day, RunnerPart part)> CollectCandidates(YearBase year, Stats currentStats,
		out List<(RunnerDay day, RunnerPart part, CheckExamplesResult.Failed failed)> failedExamples)
	{
		var dayExampleResults =
			AnsiConsole.Status()
				.Start("Checking if there are days that are implemented but not solved yet", ctx =>
				{
					var notSolvedDays =
						year.Parts.OrderByDescending(day => day.Num)
							.Select(day =>
							{
								if (!currentStats.IsSolved(Day.Create(day.Num), Part._1))
									return (day, day.Part1);
								if (!currentStats.IsSolved(Day.Create(day.Num), Part._2))
									return (day, day.Part2);

								return ((RunnerDay, RunnerPart)?)null;
							}).Where(x => x.HasValue)
							.Select(x => x!.Value)
							.ToList();

					if (notSolvedDays.Count == 0)
					{
						AnsiConsole.MarkupLine("[green bold]There are no days that are not solved yet[/]");
						return new List<(RunnerDay, RunnerPart, CheckExamplesResult)>();
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
								CheckExamplesResult.AllCorrect => "[green bold]all correct[/]",
								CheckExamplesResult.SkipNoExamples => "[green bold]no examples[/]",
								CheckExamplesResult.Failed failed => $"[red]failed {failed.FailedExamples.Count} examples[/]",
								CheckExamplesResult.NoExamples => "[grey]no examples[/]",
								CheckExamplesResult.NotImplemented => "[grey]not implemented[/]",
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
			if (result is CheckExamplesResult.Failed failed)
				return (day, part, failed).ToNullable();
			return null;
		}).ToList();

		return dayExampleResults
			.Choose(x =>
			{
				var (day, part, result) = x;
				return result is CheckExamplesResult.AllCorrect or CheckExamplesResult.SkipNoExamples
					? (day, part).ToNullable()
					: null;
			}).ToList();
	}


	private static CheckExamplesResult CheckExamples(RunnerPart part)
	{
		var examples = part.Part.GetExamples().ToList();
		if (examples.Count == 0 && part.Part.Settings.BypassNoExamples)
			return new CheckExamplesResult.SkipNoExamples();
		if (examples.Count == 0)
			return new CheckExamplesResult.NoExamples();
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
				return new CheckExamplesResult.NotImplemented();
			}
			catch (Exception e)
			{
				failedExamples.Add((example, null, e));
			}
		}

		if (failedExamples.Count == 0)
			return new CheckExamplesResult.AllCorrect();
		return new CheckExamplesResult.Failed(failedExamples);
	}*/
}