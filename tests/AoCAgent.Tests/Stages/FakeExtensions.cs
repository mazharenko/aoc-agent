namespace AoCAgent.Tests.Stages;

internal static class FakeExtensions
{
	public static RunnerPart RunnerPart1(this IPart part) => new(1, part);
	public static RunnerPart RunnerPart2(this IPart part) => new(2, part);

	public static NamedExample Named(this IExample<object> example, string name)
	{
		return new NamedExample(name, example);
	}

	public static YearBase WithDay(this YearBase year, int num, IPart part1, IPart part2)
	{
		year.Days.Add(new RunnerDay(num, part1.RunnerPart1(), part2.RunnerPart2()));
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
	public static IExample<T> Create<T>(T expectation, T result) => Create(expectation, () => result);
	public static IExample<T> Create<T>(T expectation, T result, string resultFormatted) => Create(expectation, () => result, () => resultFormatted);
	public static IExample<T> Create<T>(T expectation, Func<T> resultFunc) => Create(expectation, resultFunc, () => resultFunc()?.ToString());
	public static IExample<T> Create<T>(T expectation, Func<T> resultFunc, Func<string?> resultFormattedFunc)
	{
		return A.Fake<IExample<T>>(o =>
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