using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Testing;

namespace AoCAgent.Tests.Stages;

internal class CheckExamplesStageTests
{
	[Test]
	public async Task Should_Return_Empty_When_No_Not_Solved()
	{
		var year = FakeYear.Default
			.WithPart(1, 1, FakePart.Strict)
			.WithPart(1, 2, FakePart.Strict)
			.WithPart(2, 1, FakePart.Strict)
			.WithPart(2, 2, FakePart.Strict);
		
		var stats = new Stats()
			.Solved(DayNum.Create(1), PartNum._1) // solved and implemented
			.Solved(DayNum.Create(1), PartNum._2) // solved and implemented
			.Solved(DayNum.Create(2), PartNum._1) // solved and implemented
			.Solved(DayNum.Create(2), PartNum._2) // solved and implemented
			.Solved(DayNum.Create(3), PartNum._1) // solved and not implemented
			.Solved(DayNum.Create(3), PartNum._2) // solved and not implemented
			.NotSolved(DayNum.Create(4), PartNum._1) // neither solved nor implemented
			.NotSolved(DayNum.Create(4), PartNum._2); // neither solved nor implemented

		var services = new ServiceCollection();
		var console = new TestConsole().EmitAnsiSequences();
		services.AddSingleton<IAnsiConsole>(console);
		
		var stage = new CheckExamplesStage(new RunnerContext(year, services.BuildServiceProvider()), 
			A.Fake<ICheckPartExamplesSubStage>(o => o.Strict()));
		stage.CheckExamples(stats).Should().BeEmpty();
		await Verify(console.Output);
	}

	[Test]
	public async Task Should_Call_SubStage_For_Not_Solved()
	{
		var year = FakeYear.Default;

		void SetImplemented(RunnerPart part) =>
			year.Parts.Add(part);

		var stats = new Stats();
		void SetSolved(RunnerPart part) => stats.Solved(part.Day, part.PartNum);
		void SetNotSolved(RunnerPart part) => stats.NotSolved(part.Day, part.PartNum);

		var solvedAndImplemented = new RunnerPart(DayNum.Create(1), PartNum._1, A.Fake<IPart>());
		SetSolved(solvedAndImplemented);
		SetImplemented(solvedAndImplemented);

		var notSolvedButImplemented = new RunnerPart(DayNum.Create(1), PartNum._2, A.Fake<IPart>());
		SetNotSolved(notSolvedButImplemented);
		SetImplemented(notSolvedButImplemented);
		var notSolvedButImplementedExpectedResult = new CheckExamplesResult.AllCorrect();

		var solvedAndNotImplemented = new RunnerPart(DayNum.Create(4), PartNum._1, A.Fake<IPart>());
		SetSolved(solvedAndNotImplemented);
		
		var neitherSolvedNorImplemented = new RunnerPart(DayNum.Create(4), PartNum._2, A.Fake<IPart>());
		SetNotSolved(neitherSolvedNorImplemented);

		var services = new ServiceCollection();
		var console = new TestConsole().EmitAnsiSequences();
		services.AddSingleton<IAnsiConsole>(console);

		var subStage = A.Fake<ICheckPartExamplesSubStage>();
		A.CallTo(() => subStage.CheckExamples(notSolvedButImplemented))
			.Returns(notSolvedButImplementedExpectedResult);

		var stage = new CheckExamplesStage(new RunnerContext(year, services.BuildServiceProvider()), subStage);
		var result = stage.CheckExamples(stats);
		
		result.Should().NotContain(x => x.part == solvedAndImplemented, "it is solved but implemented");
		result.Should().NotContain(x => x.part == neitherSolvedNorImplemented, "it is neither solved nor implemented");
		result.Should().NotContain(x => x.part == solvedAndNotImplemented, "it is solved and not implemented");
		result.Should().Contain((notSolvedButImplemented, notSolvedButImplementedExpectedResult), "it is solved and implemented");
		
		await Verify(console.Output);
	}
}