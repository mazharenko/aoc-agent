using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Testing;

namespace AoCAgent.Tests.Stages;

internal class CheckPartExamplesSubStageTests
{
	[Test]
	public async Task Should_Return_NoExamples_When_No_Examples()
	{
		var part = FakePart.Strict;
		A.CallTo(() => part.Settings)
			.Returns(new Settings
			{
				ManualInterpretation = false,
				BypassNoExamples = false
			});
		A.CallTo(() => part.GetExamples()).Returns([]);

		var result = RunExamples(part);
		result.Value.Should().Be(new CheckExamplesResult.NoExamples());
		await Verify(result.Console);
	}

	[Test]
	public async Task Should_Return_SkipNoExamples_When_No_Examples_And_Bypass()
	{
		var part = FakePart.Strict;
		A.CallTo(() => part.Settings)
			.Returns(new Settings
			{
				ManualInterpretation = false,
				BypassNoExamples = true
			});
		A.CallTo(() => part.GetExamples()).Returns([]);

		var result = RunExamples(part);
		result.Value.Should().Be(new CheckExamplesResult.SkipNoExamples());
		await Verify(result.Console);
	}

	[Test]
	public async Task Should_Return_AllCorrect_When_All_Correct()
	{
		var part = FakePart.Strict;
		A.CallTo(() => part.Settings)
			.Returns(new Settings
			{
				ManualInterpretation = false,
				BypassNoExamples = false
			});

		var examples = new List<NamedExample>();

		var intExpectation = 456988;
		var intResult = intExpectation;
		examples.Add(
			FakeExample.Create(intExpectation, intResult).Named("exampleValueTypeViaAdapter")
		);

		var referenceTypeExpectation = A.Fake<IType>();
		var referenceTypeResult = A.Fake<IType>();
		A.CallTo(() => referenceTypeResult.Equals(referenceTypeExpectation)).Returns(true);
		examples.Add(FakeExample.Create(referenceTypeExpectation, referenceTypeResult).Named("exampleReferenceType"));

		A.CallTo(() => part.GetExamples()).Returns(examples);

		var result = RunExamples(part);
		result.Value.Should().Be(new CheckExamplesResult.AllCorrect());
		await Verify(result.Console);
	}

	[Test]
	public async Task Should_Return_NotImplemented_When_Any_Not_Implemented()
	{
		var part = FakePart.Strict;
		A.CallTo(() => part.Settings)
			.Returns(new Settings
			{
				ManualInterpretation = false,
				BypassNoExamples = false
			});

		var examples = new List<NamedExample>
		{
			FakeExample.Create("42", "42")
				.Named("correctExample"),
			FakeExample.Create("42", "41")
				.Named("failedExample"),
			FakeExample.Create("42", () => throw new NotImplementedException())
				.Named("notImplementedExample")
		};

		A.CallTo(() => part.GetExamples()).Returns(examples);

		var result = RunExamples(part);
		result.Value.Should().Be(new CheckExamplesResult.NotImplemented());
		await Verify(result.Console);
	}

	[Test]
	public async Task Should_Return_Failed_For_Failed_Examples()
	{
		var part = FakePart.Strict;
		A.CallTo(() => part.Settings)
			.Returns(new Settings
			{
				ManualInterpretation = false,
				BypassNoExamples = false
			});


		var failedExample = FakeExample.Create("42", "41")
			.Named("failedExample");
		var correctExample = FakeExample.Create("42", "42")
			.Named("correctExample");
		var failedWithExceptionExample = FakeExample.Create("42", () => throw new Exception("failed with exception"))
			.Named("failedWithExceptionExample");

		var customTypeExpectation = A.Fake<IType>();
		var customTypeResult = A.Fake<IType>();
		A.CallTo(() => customTypeResult.Equals(customTypeExpectation)).Returns(false);
		var failedCustomTypeExample =
			FakeExample.Create(customTypeExpectation, customTypeResult, "actualCustomTypeFormatted")
				.Named("failedWithExceptionExample");
		
		var examples = new List<NamedExample>
		{
			failedExample,
			correctExample,
			failedWithExceptionExample,
			failedCustomTypeExample
		};

		A.CallTo(() => part.GetExamples()).Returns(examples);
		var result = RunExamples(part);
		result.Value.Should().BeOfType<CheckExamplesResult.Failed>()
			.Which.FailedExamples.Should().SatisfyRespectively(
				x =>
				{
					using (new AssertionScope())
					{
						x.example.Should().BeSameAs(failedExample);
						x.actual.Should().Be("41");
						x.exception.Should().BeNull();
					}
				},
				x =>
				{
					using (new AssertionScope())
					{
						x.example.Should().BeSameAs(failedWithExceptionExample);
						x.actual.Should().BeNull();
						x.exception.Should().NotBeNull();
						x.exception?.Message.Should().Be("failed with exception");
					}
				},
				x =>
				{
					using (new AssertionScope())
					{
						x.example.Should().BeSameAs(failedCustomTypeExample);
						x.actual.Should().Be("actualCustomTypeFormatted");
						x.exception.Should().BeNull();
					}
				}
			);
		
		await Verify(result.Console);
	}

	private static ConsoleEffect<CheckExamplesResult> RunExamples(IPart part)
	{
		var services = new ServiceCollection();
		var console = new TestConsole().EmitAnsiSequences();
		services.AddSingleton<IAnsiConsole>(console);

		var stage = new CheckPartExamplesSubStage(new RunnerContext(FakeYear.Default, services.BuildServiceProvider()));
		var value = stage.CheckExamples(part.RunnerPart(10, 2));
		return new ConsoleEffect<CheckExamplesResult>
		{
			Console = console.Output, 
			Value = value
		};
	}

	// ReSharper disable once MemberCanBePrivate.Global
	internal interface IType;
}