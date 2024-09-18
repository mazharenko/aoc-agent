using mazharenko.AoCAgent.Base;
using mazharenko.AoCAgent.Client;
using mazharenko.AoCAgent.Misc;
using Spectre.Console;

namespace mazharenko.AoCAgent.Stages;

internal class CheckExamplesStage(RunnerContext runnerContext)
{
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
	}

	public List<(RunnerDay, RunnerPart, CheckExamplesResult)> CheckExamples(Stats currentStats)
	{
		var notSolvedDays =
			new Status(runnerContext.Console)
				.Start("Checking if there are days and parts that are implemented but not solved yet", ctx =>
				{
					var notSolvedParts =
						runnerContext.Year.Days.OrderByDescending(day => day.Num)
							.Choose(day =>
							{
								if (!currentStats.IsSolved(Day.Create(day.Num), Part._1))
									return (day, day.Part1);
								if (!currentStats.IsSolved(Day.Create(day.Num), Part._2))
									return (day, day.Part2);

								return ((RunnerDay, RunnerPart)?)null;
							})
							.ToList();
					return notSolvedParts;
				});

		if (notSolvedDays.Count == 0)
		{
			runnerContext.Console.MarkupLine("[green bold]There are no days that are not solved yet[/]");
			return [];
		}

		runnerContext.Console.MarkupLine(
			$"Found [yellow bold]{notSolvedDays.Count}[/] not solved days and parts: " +
			string.Join(", ", notSolvedDays.Select(x => $"{x.Item1.Num:00}/{x.Item2.Num}"))
		);
		var dayExampleResults =
			new Status(runnerContext.Console)
				.Start("Calculating examples", ctx =>
				{
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
							runnerContext.Console.MarkupLine($"Day {day.Num:00} Part {part.Num} - {status}");
							return (day, part, result);
						}).ToList();

					return dayExampleResults;
				});

		return dayExampleResults;
		var failedExamples = dayExampleResults
			.Choose(x =>
			{
				var (day, part, result) = x;
				if (result is CheckExamplesResult.Failed failed)
					return (day, part, failed).ToNullable();
				return null;
			}).ToList();
	}
}