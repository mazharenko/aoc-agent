using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace mazharenko.AoCAgent.Generator.Sources;

internal readonly record struct PartSource(
	ClassDeclarationSyntax PartClass,
	INamedTypeSymbol PartType,
	ITypeSymbol InputType,
	bool IsStringInput,
	ITypeSymbol ResType,
	bool IsStringRes,
	bool BypassNoExamples
)
{
	public bool Equals(PartSource other)
	{
		return PartClass.Equals(other.PartClass) 
		       && SymbolEqualityComparer.Default.Equals(PartType, other.PartType) 
		       && SymbolEqualityComparer.Default.Equals(InputType, other.InputType)
		       && IsStringInput == other.IsStringInput
		       && SymbolEqualityComparer.Default.Equals(ResType, other.ResType)
		       && IsStringRes == other.IsStringRes
		       && BypassNoExamples == other.BypassNoExamples;
	}

	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = PartClass.GetHashCode();
			hashCode = (hashCode * 397) ^ SymbolEqualityComparer.Default.GetHashCode(PartType);
			hashCode = (hashCode * 397) ^ SymbolEqualityComparer.Default.GetHashCode(InputType);
			hashCode = (hashCode * 397) ^ IsStringInput.GetHashCode();
			hashCode = (hashCode * 397) ^ SymbolEqualityComparer.Default.GetHashCode(ResType);
			hashCode = (hashCode * 397) ^ IsStringRes.GetHashCode();
			hashCode = (hashCode * 397) ^ BypassNoExamples.GetHashCode();
			return hashCode;
		}
	}
}