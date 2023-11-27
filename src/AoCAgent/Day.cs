namespace mazharenko.AoCAgent;

internal record Day
{
	public int Num { get; }

	private Day(int num)
	{
		Num = num;
	}

	public static Day Create(int num)
	{
		return TryCreate(num) ?? throw new ArgumentOutOfRangeException();
	}

	public static Day? TryCreate(int num)
	{
		return num is >= 1 and <= 25 ? new Day(num) : null;
	}

	public static implicit operator int(Day day)
	{
		return day.Num;
	}
}