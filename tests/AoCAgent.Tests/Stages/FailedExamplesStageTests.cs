using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Testing;

namespace AoCAgent.Tests.Stages;

public class FailedExamplesStageTests
{
	[Test]
	public async Task Should_Return_False_When_No_Failed()
	{
		var exampleResults = new List<(RunnerPart, CheckExamplesResult)>
		{
			(FakePart.Strict.RunnerPart(1, 1), 
				new CheckExamplesResult.AllCorrect()),
			(FakePart.Strict.RunnerPart(2, 1), 
				new CheckExamplesResult.NoExamples()),
			(FakePart.Strict.RunnerPart(3, 1), 
				new CheckExamplesResult.NotImplemented()),
			(FakePart.Strict.RunnerPart(4, 1), 
				new CheckExamplesResult.SkipNoExamples())
		};

		var services = new ServiceCollection();
		var console = new TestConsole().EmitAnsiSequences();
		services.AddSingleton<IAnsiConsole>(console);

		var stage = new FailedExamplesStage(new RunnerContext(FakeYear.Default, services.BuildServiceProvider()));
		var result = stage.ReportFailedExamples(exampleResults);
		
		result.AtLeastOneFailedExample.Should().BeFalse();
		
		await Verify(console.Output);
	}

	[Test]
	public async Task Should_Print_All_Failed_Examples()
	{
		var failedExample = A.Fake<IExample>();
		A.CallTo(() => failedExample.ExpectationFormatted).Returns("failed expected");
		var failedExampleResult = 
			(failedExample.Named("failedExample"), "failed actual", (Exception?)null);

		var failedWithExceptionExample = A.Fake<IExample>();
		A.CallTo(() => failedWithExceptionExample.ExpectationFormatted).Returns("failed with ex expected");
		var failedWithExceptionExampleResult =
			(failedWithExceptionExample.Named("failedWithExceptionExample"), (string?)null, new Exception("exception message"));
		

		var exampleResults = new List<(RunnerPart, CheckExamplesResult)>
		{
			(FakePart.Strict.RunnerPart(1, 1),
				new CheckExamplesResult.Failed([failedExampleResult, failedWithExceptionExampleResult])),
			(FakePart.Strict.RunnerPart(1, 2),
				new CheckExamplesResult.Failed([failedExampleResult, failedWithExceptionExampleResult]))
		};
		

		var services = new ServiceCollection();
		var console = new TestConsole().EmitAnsiSequences();
		services.AddSingleton<IAnsiConsole>(console);

		var stage = new FailedExamplesStage(new RunnerContext(FakeYear.Default, services.BuildServiceProvider()));
		var result = stage.ReportFailedExamples(exampleResults);
		result.AtLeastOneFailedExample.Should().BeTrue();

		await Verify(console.Output);
	}

	// ReSharper disable once MemberCanBePrivate.Global
	internal interface IType;
}