using mazharenko.AoCAgent.Base;

namespace AoCAgent.Tests.Stages;

internal class TestYear(int year) : YearBase
{
	public TestYear() : this(2024)
	{
	}

	public override int Year { get; } = year;
}

/*
internal class TestPart : IPart
{
	public IEnumerable<NamedExample> GetExamples()
	{
		throw new NotImplementedException();
	}

	public string SolveString(string input)
	{
		throw new NotImplementedException();
	}

	public Settings Settings { get; }
}*/