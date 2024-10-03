using System.ComponentModel;

namespace mazharenko.AoCAgent;

[EditorBrowsable(EditorBrowsableState.Never)]
public record PartNum
{
	public int Num { get; }

	private PartNum(int num)
	{
		Num = num;
	}

	public static PartNum _1 { get; } = new(1);
	public static PartNum _2 { get; } = new(2);

	public static PartNum Create(int num)
	{
		return TryCreate(num) ?? throw new ArgumentOutOfRangeException();
	}

	public static PartNum? TryCreate(int num)
	{
		return num switch
		{
			1 => _1,
			2 => _2,
			_ => null
		};
	}

	public static implicit operator int(PartNum part)
	{
		return part.Num;
	}

	public override string ToString()
	{
		return Num.ToString();
	}
}