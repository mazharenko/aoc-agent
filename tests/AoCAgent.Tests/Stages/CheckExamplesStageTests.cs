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
			.WithDay(1, FakePart.Strict, FakePart.Strict)
			.WithDay(2, FakePart.Strict, FakePart.Strict);
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
			A.Fake<ICheckPartExamplesSubStage>(o => o.Strict()));
		stage.CheckExamples(stats).Should().BeEmpty();
		await Verify(console.Output);
	}

	[Test]
	// todo refactor integer and typed day and part nums and this will become better?
	// todo emphasize that the order is checked?
	public async Task Should_Call_SubStage_For_Not_Solved()
	{
		var year = FakeYear.Default;

		void SetImplemented(int dayNum, RunnerPart part1, RunnerPart part2) =>
			year.Days.Add(new RunnerDay(dayNum, part1, part2));

		var stats = new Stats();
		void SetSolved((int, RunnerPart) x) => stats.Add(Day.Create(x.Item1), Part.Create(x.Item2.Num), true);
		void SetNotSolved((int, RunnerPart) x) => stats.Add(Day.Create(x.Item1), Part.Create(x.Item2.Num), false);

		var solvedAndImplemented = (day: 1, part: new RunnerPart(1, A.Fake<IPart>()));
		SetSolved(solvedAndImplemented);
		// expected to skip

		var notSolvedButImplemented = (day: 1, part: new RunnerPart(2, A.Fake<IPart>()));
		SetNotSolved(notSolvedButImplemented);
		SetImplemented(1, solvedAndImplemented.part, notSolvedButImplemented.part);
		var notSolvedButImplementedExpectedResult = new CheckExamplesResult.AllCorrect();

		var notSolvedButImplemented2 = (day: 2, part: new RunnerPart(1, A.Fake<IPart>()));
		SetNotSolved(notSolvedButImplemented2);
		var notSolvedButImplemented2ExpectedResult = new CheckExamplesResult.AllCorrect();

		var notSolvedButImplemented3 = (day: 2, part: new RunnerPart(2, A.Fake<IPart>()));
		SetNotSolved(notSolvedButImplemented3);
		SetImplemented(2, notSolvedButImplemented2.part, notSolvedButImplemented3.part);
		var notSolvedButImplemented3ExpectedResult = new CheckExamplesResult.AllCorrect();

		var solvedAndNotImplemented = (day: 4, part: new RunnerPart(1, A.Fake<IPart>()));
		SetSolved(solvedAndNotImplemented);
		var neitherSolvedNorImplemented = (day: 4, part: new RunnerPart(2, A.Fake<IPart>()));
		SetNotSolved(neitherSolvedNorImplemented);
		// expected to skip

		var services = new ServiceCollection();
		var console = new TestConsole().EmitAnsiSequences();
		services.AddSingleton<IAnsiConsole>(console);

		var subStage = A.Fake<ICheckPartExamplesSubStage>(o => o.Strict());
		A.CallTo(() => subStage.CheckExamples(notSolvedButImplemented.day, notSolvedButImplemented.part))
			.Returns(notSolvedButImplementedExpectedResult);
		A.CallTo(() => subStage.CheckExamples(notSolvedButImplemented2.day, notSolvedButImplemented2.part))
			.Returns(notSolvedButImplemented2ExpectedResult);
		A.CallTo(() => subStage.CheckExamples(notSolvedButImplemented3.day, notSolvedButImplemented3.part))
			.Returns(notSolvedButImplemented3ExpectedResult);

		var stage = new CheckExamplesStage(new RunnerContext(year, services.BuildServiceProvider()), subStage);
		stage.CheckExamples(stats).Should().SatisfyRespectively(
			res =>
			{
				res.Item1.Should().Be(notSolvedButImplemented2.day);
				res.Item2.Should().Be(notSolvedButImplemented2.part);
				res.Item3.Should().BeSameAs(notSolvedButImplemented2ExpectedResult);
			},
			res =>
			{
				res.Item1.Should().Be(notSolvedButImplemented3.day);
				res.Item2.Should().Be(notSolvedButImplemented3.part);
				res.Item3.Should().BeSameAs(notSolvedButImplemented3ExpectedResult);
			},
			res =>
			{
				res.Item1.Should().Be(notSolvedButImplemented.day);
				res.Item2.Should().Be(notSolvedButImplemented.part);
				res.Item3.Should().BeSameAs(notSolvedButImplementedExpectedResult);
			}
		);
		await Verify(console.Output);
	}
}