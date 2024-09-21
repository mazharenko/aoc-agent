using mazharenko.AoCAgent.Base;
using Spectre.Console;

namespace mazharenko.AoCAgent.Stages;

internal class CheckPartExamplesSubStage(RunnerContext runnerContext)
{
	public CheckExamplesResult CheckExamples(int dayNum, RunnerPart part)
	{
		var examples = part.Part.GetExamples().ToList();
		if (examples.Count == 0 && part.Part.Settings.BypassNoExamples)
		{
			runnerContext.Console.MarkupLine($"Day {dayNum:00} Part {part.Num} - [green bold]no examples[/]");
			return new CheckExamplesResult.SkipNoExamples();
		}

		if (examples.Count == 0)
		{
			runnerContext.Console.MarkupLine($"Day {dayNum:00} Part {part.Num} - [grey]no examples[/]");
			return new CheckExamplesResult.NoExamples();
		}

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
				runnerContext.Console.MarkupLine($"Day {dayNum:00} Part {part.Num} - [grey]not implemented[/]");
				return new CheckExamplesResult.NotImplemented();
			}
			catch (Exception e)
			{
				failedExamples.Add((example, null, e));
			}
		}

		if (failedExamples.Count == 0)
		{
			runnerContext.Console.MarkupLine($"Day {dayNum:00} Part {part.Num} - [green bold]all correct[/]");
			return new CheckExamplesResult.AllCorrect();
		}

		runnerContext.Console.MarkupLine($"Day {dayNum:00} Part {part.Num} - [red]failed {failedExamples.Count} examples[/]");
		return new CheckExamplesResult.Failed(failedExamples);
	}
}