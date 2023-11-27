using System.Collections;
using System.Collections.Immutable;

namespace mazharenko.AoCAgent.Generator.Sources;

internal readonly record struct YearSource(
	YearClass YearClass,
	ImmutableArray<DaySource> DaySources
)
{
	public bool Equals(YearSource other)
	{
		return Equals(YearClass, other.YearClass)
		       && StructuralComparisons.StructuralEqualityComparer.Equals(DaySources, other.DaySources);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = YearClass.GetHashCode();
			hashCode = (hashCode * 397) ^ StructuralComparisons.StructuralEqualityComparer.GetHashCode(DaySources);
			return hashCode;
		}
	}
}