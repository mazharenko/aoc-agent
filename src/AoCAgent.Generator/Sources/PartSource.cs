using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace mazharenko.AoCAgent.Generator.Sources;

internal readonly record struct PartSource()
{
	public bool Equals(PartSource other)
	{
		return PartClass.Equals(other.PartClass)
		       && SymbolEqualityComparer.Default.Equals(PartType, other.PartType)
		       && SymbolEqualityComparer.Default.Equals(InputType, other.InputType)
		       && IsStringInput == other.IsStringInput
		       && SymbolEqualityComparer.Default.Equals(ResType, other.ResType)
		       && IsStringRes == other.IsStringRes
		       && BypassNoExamples == other.BypassNoExamples
		       && ManualInput == other.ManualInput
		       && ManualInterpretation == other.ManualInterpretation
		       ;
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
			hashCode = (hashCode * 397) ^ ManualInput.GetHashCode();
			hashCode = (hashCode * 397) ^ ManualInterpretation.GetHashCode();
			return hashCode;
		}
	}

	public required ClassDeclarationSyntax PartClass { get; init; } 
	public required INamedTypeSymbol PartType { get; init; }
	public required ITypeSymbol InputType { get; init; }
	public required bool IsStringInput { get; init; }
	public required ITypeSymbol ResType { get; init; }
	public required bool IsStringRes { get; init; }
	public required bool BypassNoExamples { get; init; }
	public required bool ManualInput { get; init; }
	public required bool ManualInterpretation { get; init; }
}