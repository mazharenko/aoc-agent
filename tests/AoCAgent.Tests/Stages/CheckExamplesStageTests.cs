using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Testing;

namespace AoCAgent.Tests.Stages;

public class CheckExamplesStageTests
{
	[Test]
	public async Task Should_Return_Empty_When_No_Not_Solved()
	{
		var year = new TestYear
		{
			Days =
			{
				DayFactory.Create(1, FakePart.Strict, FakePart.Strict),
				DayFactory.Create(2, FakePart.Strict, FakePart.Strict)
			}
		};
		var stats = new Stats
		{
			{ Day.Create(1), Part._1, true }, // solved and implemented
			{ Day.Create(1), Part._2, true }, // solved and implemented
			{ Day.Create(2), Part._1, true }, // solved and implemented
			{ Day.Create(2), Part._2, true }, // solved and implemented
			{ Day.Create(3), Part._1, true }, // solved and not implemented
			{ Day.Create(3), Part._2, true }, // solved and not implemented
			{ Day.Create(4), Part._1, false }, // neither solved nor implemented
			{ Day.Create(4), Part._2, false }, // neither solved nor implemented
		};

		var services = new ServiceCollection();
		var console = new TestConsole().EmitAnsiSequences();
		services.AddSingleton<IAnsiConsole>(console);
		
		var stage = new CheckExamplesStage(new RunnerContext(year, services.BuildServiceProvider()), 
			A.Fake<CheckPartExamplesSubStage>(o => o.Strict()));
		stage.CheckExamples(stats).Should().BeEmpty();
		await Verify(console.Output);
	}
}