namespace AoCAgent.Tests.Stages;

internal static class FakeExtensions
{
	public static RunnerPart RunnerPart(this IPart part, int dayNum, int partNum) 
		=> new(DayNum.Create(dayNum), PartNum.Create(partNum), part);

	public static NamedExample Named(this IExample example, string name)
	{
		return new NamedExample(name, example);
	}

	public static YearBase WithPart(this YearBase year, int dayNum, int partNum, IPart part)
	{
		year.Parts.Add(part.RunnerPart(dayNum, partNum));
		return year;
	}
}

internal static class FakeYear
{
	public static YearBase Default => A.Fake<YearBase>(o =>
		o.ConfigureFake(y =>
		{
			A.CallTo(() => y.Year).Returns(2020);
		})
	);
}

internal static class FakePart
{
	public static IPart Strict => A.Fake<IPart>(o => o.Strict());
}

internal static class FakeExample
{
	public static IExample Create(object expectation, object result) => Create(expectation, () => result);
	public static IExample Create(object expectation, object result, string resultFormatted) => Create(expectation, () => result, () => resultFormatted);
	public static IExample Create(object expectation, Func<object> resultFunc) => Create(expectation, resultFunc, () => resultFunc()?.ToString());
	public static IExample Create(object expectation, Func<object> resultFunc, Func<string?> resultFormattedFunc)
	{
		return A.Fake<IExample>(o =>
			o.ConfigureFake(ex =>
			{
				string formatted;
				A.CallTo(() => ex.Expectation).Returns(expectation);
				A.CallTo(() => ex.RunFormat(out formatted))
					.ReturnsLazily(resultFunc)
					.AssignsOutAndRefParametersLazily(_ => [resultFormattedFunc()]);
			})
		);
	}
}