namespace AoCAgent.Tests.Stages;

internal static class FakeExtensions
{
	public static RunnerPart RunnerPart1(this IPart part) => new(1, part);
	public static RunnerPart RunnerPart2(this IPart part) => new(2, part);
}

public static class FakePart
{
	public static IPart Strict => A.Fake<IPart>(o => o.Strict());
}

public static class DayFactory
{
	public static RunnerDay Create(int num, IPart part1, IPart part2) =>
		new(num, part1.RunnerPart1(), part2.RunnerPart2());
}