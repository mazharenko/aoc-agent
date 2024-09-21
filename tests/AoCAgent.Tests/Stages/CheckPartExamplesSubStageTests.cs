using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Testing;

namespace AoCAgent.Tests.Stages;

public class CheckPartExamplesSubStageTests
{
	[Test]
	public async Task Should_Return_NoExamples_When_No_Examples()
	{
		var dayNum = 10;
		var part = FakePart.Strict;
		A.CallTo(() => part.Settings)
			.Returns(new Settings
			{
				ManualInterpretation = false,
				BypassNoExamples = false
			});
		A.CallTo(() => part.GetExamples()).Returns([]);
		
		var services = new ServiceCollection();
		var console = new TestConsole().EmitAnsiSequences();
		services.AddSingleton<IAnsiConsole>(console);

		var stage = new CheckPartExamplesSubStage(new RunnerContext(new TestYear(), services.BuildServiceProvider()));
		stage.CheckExamples(dayNum, part.RunnerPart2()).Should().Be(new CheckExamplesResult.NoExamples());
		await Verify(console.Output);
	}
	
	[Test]
	public async Task Should_Return_SkipNoExamples_When_No_Examples_And_Bypass()
	{
		var dayNum = 10;
		var part = A.Fake<IPart>();
		A.CallTo(() => part.Settings)
			.Returns(new Settings
			{
				ManualInterpretation = false,
				BypassNoExamples = true
			});
		A.CallTo(() => part.GetExamples()).Returns([]);
		
		var services = new ServiceCollection();
		var console = new TestConsole().EmitAnsiSequences();
		services.AddSingleton<IAnsiConsole>(console);

		var stage = new CheckPartExamplesSubStage(new RunnerContext(new TestYear(), services.BuildServiceProvider()));
		stage.CheckExamples(dayNum, part.RunnerPart2()).Should().Be(new CheckExamplesResult.SkipNoExamples());
		await Verify(console.Output);
	}
}