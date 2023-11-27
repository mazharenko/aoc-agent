using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace mazharenko.AoCAgent.Generator.Mics;

internal static class CodeGeneratedAttribute
{
	public static readonly string AsString = $"""[System.CodeDom.Compiler.GeneratedCodeAttribute("{ThisAssembly.Info.Title}", "{ThisAssembly.Info.InformationalVersion}")]""";
	
	public static readonly AttributeListSyntax AsSyntax = 
		ParseSyntaxTree(AsString).GetRoot().DescendantNodesAndSelf().OfType<AttributeListSyntax>().First();
}
