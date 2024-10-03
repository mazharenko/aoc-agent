using mazharenko.AoCAgent.Base;
using Spectre.Console;

namespace mazharenko.AoCAgent.Stages;

internal interface ICheckPartExamplesSubStage
{
	CheckExamplesResult CheckExamples(RunnerPart part);
}

internal class CheckPartExamplesSubStage(RunnerContext runnerContext) : ICheckPartExamplesSubStage
{
	public CheckExamplesResult CheckExamples(RunnerPart part)
	{
		var examples = part.Part.GetExamples().ToList();
		if (examples.Count == 0 && part.Part.Settings.BypassNoExamples)
		{
			runnerContext.Console.MarkupLine($"Day {part.Day:00} Part {part.PartNum} - [green bold]no examples[/]");
			return new CheckExamplesResult.SkipNoExamples();
		}

		if (examples.Count == 0)
		{
			runnerContext.Console.MarkupLine($"Day {part.Day:00} Part {part.PartNum} - [grey]no examples[/]");
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
				runnerContext.Console.MarkupLine($"Day {part.Day:00} Part {part.PartNum} - [grey]not implemented[/]");
				return new CheckExamplesResult.NotImplemented();
			}
			catch (Exception e)
			{
				failedExamples.Add((example, null, e));
			}
		}

		if (failedExamples.Count == 0)
		{
			runnerContext.Console.MarkupLine($"Day {part.Day:00} Part {part.PartNum} - [green bold]all correct[/]");
			return new CheckExamplesResult.AllCorrect();
		}

		runnerContext.Console.MarkupLine($"Day {part.Day:00} Part {part.PartNum} - [red]failed {failedExamples.Count} examples[/]");
		return new CheckExamplesResult.Failed(failedExamples);
	}
}