using System.ComponentModel;

namespace mazharenko.AoCAgent;

[EditorBrowsable(EditorBrowsableState.Never)]
public record DayNum : IComparable<DayNum>
{
	public int CompareTo(DayNum? other)
	{
		if (ReferenceEquals(this, other)) return 0;
		if (other is null) return 1;
		return Num.CompareTo(other.Num);
	}

	public int Num { get; }

	private DayNum(int num)
	{
		Num = num;
	}

	public static DayNum Create(int num)
	{
		return TryCreate(num) ?? throw new ArgumentOutOfRangeException();
	}

	public static DayNum? TryCreate(int num)
	{
		return num is >= 1 and <= 25 ? new DayNum(num) : null;
	}

	public static implicit operator int(DayNum day)
	{
		return day.Num;
	}

	public override string ToString()
	{
		return $"{Num:00}";
	}
}