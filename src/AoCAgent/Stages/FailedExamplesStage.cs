using mazharenko.AoCAgent.Misc;
using Spectre.Console;

namespace mazharenko.AoCAgent.Stages;

internal class FailedExamplesStage(RunnerContext runnerContext)
{
	public FailedExamplesResult ReportFailedExamples(List<(RunnerPart part, CheckExamplesResult result)> exampleResults)
	{
		var failedExamples = exampleResults.Choose(x =>
		{
			if (x.result is CheckExamplesResult.Failed failed)
				return (x.part.Day, x.part, failed).ToNullable();
			return null;
		}).ToList();

		if (failedExamples.Count == 0)
			return new FailedExamplesResult(false);
		
		runnerContext.Console.MarkupLine("[yellow bold]Please revise failed examples[/]");

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
				table.AddRow(day.ToString(), part.PartNum.ToString(),
					runnerExample.Name,
					runnerExample.Example.ExpectationFormatted,
					$"[red]{Markup.Escape(exception?.Message ?? actual ?? "")}[/]");
			}
		}
		runnerContext.Console.Write(table);
		return new FailedExamplesResult(true);
	}
}