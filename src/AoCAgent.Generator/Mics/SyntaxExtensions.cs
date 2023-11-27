using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace mazharenko.AoCAgent.Generator.Mics;

public static class SyntaxExtensions
{
	public static NameSyntax? GetContainingNamespace(this TypeDeclarationSyntax type)
	{
		var ns = string.Join(".", type.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().Reverse().Select(x => x.Name));
		return ns is "" ? null : ParseName(ns);
	}

	public static CompilationUnitSyntax AddDefaultUsings(this CompilationUnitSyntax syntax)
	{
		return syntax.AddUsings(
			UsingDirective(IdentifierName("mazharenko.AoCAgent.Generator")),
			UsingDirective(IdentifierName("mazharenko.AoCAgent")),
			UsingDirective(IdentifierName("mazharenko.AoCAgent.Base"))
		);
	}
}