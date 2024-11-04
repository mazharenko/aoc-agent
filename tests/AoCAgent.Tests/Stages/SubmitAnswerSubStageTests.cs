using System.Text.RegularExpressions;
using FluentAssertions.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Testing;

namespace AoCAgent.Tests.Stages;

internal class SubmitAnswerSubStageTests
{
	private static TestCaseData[] WrongAnswerCases =
	[
		new(new SubmissionResult.Incorrect(), false),
		new(new SubmissionResult.TooHigh(), false),
		new(new SubmissionResult.TooLow(), false),
		new(new SubmissionResult.Correct(), true)
	];

	[TestCaseSource(nameof(WrongAnswerCases))]
	public async Task Should_Submit_Wrong_Answer(SubmissionResult submissionResult, bool expectedResult)
	{
		const string input = "input";
		const string answer = "answer";
		var dayNum = DayNum.Create(10);
		var partNum = PartNum._1;

		var part = FakePart.Strict;
		A.CallTo(() => part.Settings)
			.Returns(new Settings
			{
				ManualInterpretation = false,
				BypassNoExamples = false
			});
		A.CallTo(() => part.SolveString(input)).Returns(answer);

		var client = A.Fake<IAoCClient>(o => o.Strict());
		A.CallTo(() => client.LoadInput(dayNum)).Returns(input);
		A.CallTo(() => client.SubmitAnswer(dayNum, partNum, answer))
			.Returns(Task.FromResult(submissionResult));

		var services = new ServiceCollection();
		var console = new TestConsole().EmitAnsiSequences();
		services.AddSingleton<IAnsiConsole>(console);
		services.AddSingleton(client);

		var stage = new SubmitAnswerSubStage(new RunnerContext(FakeYear.Default, services.BuildServiceProvider()));
		var result = await stage.CalculateAndSubmit(new RunnerPart(dayNum, partNum, part));
		result.Should().Be(expectedResult);
		await Verify(console.Output).AddScrubber(sb =>
		{
			var s = Regex.Replace(sb.ToString(), "(?<=calculated in )\\S+", "{}");
			sb.Clear();
			sb.Append(s);
		});
	}

	[Test]
	public async Task Should_Retry_When_TooRecently()
	{
		const string input = "input";
		const string answer = "answer";
		var dayNum = DayNum.Create(10);
		var partNum = PartNum._1;

		var part = FakePart.Strict;
		A.CallTo(() => part.Settings)
			.Returns(new Settings
			{
				ManualInterpretation = false,
				BypassNoExamples = false
			});
		A.CallTo(() => part.SolveString(input)).Returns(answer);
		
		var client = A.Fake<IAoCClient>(o => o.Strict());
		A.CallTo(() => client.LoadInput(dayNum)).Returns(input);
		
		A.CallTo(() => client.SubmitAnswer(dayNum, partNum, answer))
			.ReturnsNextFromSequence(
				Task.FromResult<SubmissionResult>(new SubmissionResult.TooRecently(2.Seconds())),
				Task.FromResult<SubmissionResult>(new SubmissionResult.Correct())
			);
		
		var services = new ServiceCollection();
		var console = new TestConsole().EmitAnsiSequences();
		services.AddSingleton<IAnsiConsole>(console);
		services.AddSingleton(client);
		
		var stage = new SubmitAnswerSubStage(new RunnerContext(FakeYear.Default, services.BuildServiceProvider()));
		var result = await stage.CalculateAndSubmit(new RunnerPart(dayNum, partNum, part));
		result.Should().Be(true);
		await Verify(console.Output).AddScrubber(sb =>
		{
			var s = sb.ToString()
				.ReplaceRegex("(?<=calculated in )\\S+", "{}")
				.ReplaceRegex("(?<=Waiting )\\S+(?= more)", "{}");
			sb.Clear();
			sb.Append(s);
		});
	}
}