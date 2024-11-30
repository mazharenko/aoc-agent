using mazharenko.AoCAgent.Client;
using Spectre.Console;

namespace mazharenko.AoCAgent.Stages;

 internal class CheckExamplesStage(RunnerContext runnerContext, ICheckPartExamplesSubStage checkPartExamplesSubStage)
{
	public List<(RunnerPart part, CheckExamplesResult result)> CheckExamples(Stats currentStats)
	{
		var notSolvedParts =
			new Status(runnerContext.Console)
				.Start("Checking if there are days and parts that are implemented but not solved yet", ctx =>
				{
					var notSolvedParts =
						runnerContext.Year.Parts.OrderByDescending(part => part.Day)
							.Where(part => !currentStats.IsSolved(part.Day, part.PartNum))
							.ToList();
					return notSolvedParts;
				});

		if (notSolvedParts.Count == 0)
		{
			runnerContext.Console.MarkupLine("[green bold]There are no days that are not solved yet[/]");
			return [];
		}

		runnerContext.Console.MarkupLine(
			$"Found [yellow bold]{notSolvedParts.Count}[/] not solved days and parts: " +
			string.Join(", ", notSolvedParts.Select(x => $"{x.Day:00}/{x.PartNum}"))
		);
		var dayExampleResults =
			new Status(runnerContext.Console)
				.Start("Calculating examples", ctx =>
				{
					var dayExampleResults =
						notSolvedParts.Select(part =>
						{
							var result = checkPartExamplesSubStage.CheckExamples(part);
							return (part, result);
						}).ToList();

					return dayExampleResults;
				});

		return dayExampleResults;
	}
}