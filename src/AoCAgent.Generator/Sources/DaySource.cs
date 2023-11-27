using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace mazharenko.AoCAgent.Generator.Sources;

internal readonly record struct DaySource(
	int Number,
	int? Year,
	ClassDeclarationSyntax Syntax,
	PartSource Part1,
	PartSource Part2
)
{
	public bool Equals(DaySource other)
	{
		return Number == other.Number
		       && Syntax.Equals(other.Syntax)
		       && Part1.Equals(other.Part1)
		       && Part2.Equals(other.Part2);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = Number;
			hashCode = (hashCode * 397) ^ Syntax.GetHashCode();
			hashCode = (hashCode * 397) ^ Part1.GetHashCode();
			hashCode = (hashCode * 397) ^ Part2.GetHashCode();
			return hashCode;
		}
	}
}