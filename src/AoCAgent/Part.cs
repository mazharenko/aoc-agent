namespace mazharenko.AoCAgent;

internal record Part
{
	public int Num { get; }

	private Part(int num)
	{
		Num = num;
	}

	public static Part _1 { get; } = new(1);
	public static Part _2 { get; } = new(2);

	public static Part Create(int num)
	{
		return TryCreate(num) ?? throw new ArgumentOutOfRangeException();
	}

	public static Part? TryCreate(int num)
	{
		return num switch
		{
			1 => _1,
			2 => _2,
			_ => null
		};
	}

	public static implicit operator int(Part part)
	{
		return part.Num;
	}
}