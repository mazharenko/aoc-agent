using mazharenko.AoCAgent.Base;
using mazharenko.AoCAgent.Client;
using mazharenko.AoCAgent.Misc;
using Spectre.Console;

namespace mazharenko.AoCAgent.Stages;

internal class CheckExamplesStage(RunnerContext runnerContext, CheckPartExamplesSubStage checkPartExamplesSubStage)
{
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
							var result = checkPartExamplesSubStage.CheckExamples(day.Num, part);
							return (day, part, result);
						}).ToList();

					return dayExampleResults;
				});

		return dayExampleResults;
	}
}