using mazharenko.AoCAgent.Base;
using mazharenko.AoCAgent.Client;
using mazharenko.AoCAgent.Misc;
using Spectre.Console;

namespace mazharenko.AoCAgent.Stages;

 internal class CheckExamplesStage(RunnerContext runnerContext, ICheckPartExamplesSubStage checkPartExamplesSubStage)
{
	public List<(int, RunnerPart, CheckExamplesResult)> CheckExamples(Stats currentStats)
	{
		var notSolvedDays =
			new Status(runnerContext.Console)
				.Start("Checking if there are days and parts that are implemented but not solved yet", ctx =>
				{
					var notSolvedParts =
						runnerContext.Year.Days.OrderByDescending(day => day.Num)
							.SelectMany(day => new List<(Day day, RunnerPart part)>{
								(Day.Create(day.Num), day.Part1), 
								(Day.Create(day.Num), day.Part2)
							})
							.Where(tuple => !currentStats.IsSolved(tuple.day, Part.Create(tuple.part.Num)))
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
			string.Join(", ", notSolvedDays.Select(x => $"{x.day.Num:00}/{x.part.Num}"))
		);
		var dayExampleResults =
			new Status(runnerContext.Console)
				.Start("Calculating examples", ctx =>
				{
					var dayExampleResults =
						notSolvedDays.Select(x =>
						{
							var result = checkPartExamplesSubStage.CheckExamples(x.day, x.part);
							return (x.day.Num, x.part, result);
						}).ToList();

					return dayExampleResults;
				});

		return dayExampleResults;
	}
}