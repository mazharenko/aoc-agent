using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace mazharenko.AoCAgent.Generator.Sources;

internal readonly record struct ExplicitYear(
	int Number, 
	ClassDeclarationSyntax Syntax,
	INamedTypeSymbol Type,
	bool GenerateEntryPoint
);