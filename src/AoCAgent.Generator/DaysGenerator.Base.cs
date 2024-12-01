using System.Text;
using mazharenko.AoCAgent.Generator.Mics;
using mazharenko.AoCAgent.Generator.Sources;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace mazharenko.AoCAgent.Generator;

internal partial class DaysGenerator
{
	private void GenerateBaseTypes(SourceProductionContext productionContext, DaySource day)
	{
		productionContext.CancellationToken.ThrowIfCancellationRequested();
		
		var ns = day.Syntax.GetContainingNamespace();

		var dayWithBase =
			ClassDeclaration(day.Syntax.Identifier)
				.AddAttributeLists(CodeGeneratedAttribute.AsSyntax)
				.AddAttributeLists(
					AttributeList(
						SingletonSeparatedList(
							Attribute(ParseName("JetBrains.Annotations.UsedImplicitly"))
						)
					)
				)
				.AddModifiers(
					Token(SyntaxKind.PartialKeyword)
				).WithMembers(
					List(
						GeneratePartDeclarations("Part1", day.Part1)
							.Concat(GeneratePartDeclarations("Part2", day.Part2))
							.Cast<MemberDeclarationSyntax>()
					)
				);
		var newRoot =
			CompilationUnit()
				.AddDefaultUsings()
				.WithMembers(
					ns is null
						? SingletonList<MemberDeclarationSyntax>(dayWithBase)
						: SingletonList<MemberDeclarationSyntax>(
							NamespaceDeclaration(ns)
								.AddMembers(
									dayWithBase
								)
						)
				).WithLeadingTrivia(
					AutoGeneratedComment,
					RestoreNullable
				);
		
		productionContext.AddSource($"{day.Year}.{day.Syntax.Identifier.ValueText}.Base.g.cs", SourceText.From(
			newRoot.NormalizeWhitespace(indentation: "\t").ToFullString(),
			Encoding.UTF8
		));
	}

	private static IEnumerable<MemberDeclarationSyntax> GeneratePartMembers(PartSource part)
	{
		yield return ParseMemberDeclaration(
			$$"""
			public Settings Settings { get; } = new Settings 
				{
					BypassNoExamples = {{(part.BypassNoExamples ? "true" : "false")}},
					ManualInterpretation = {{(part.ManualInterpretation ? "true" : "false")}}
				};
			""")!;
		
		if (part.IsStringRes && part.ResType.NullableAnnotation != NullableAnnotation.Annotated)
		{
			yield return ParseMemberDeclaration(
				"""
				public override string Format(string res) => res;
				"""
			)!;
		}

		var examples =
			part.PartClass.Members.OfType<FieldDeclarationSyntax>()
				.Where(field => field.Declaration.Type.ToString() == "Example")
				.SelectMany(exampleField => exampleField.Declaration.Variables)
				.ToList();

		if (examples.Count == 0)
			yield return MethodDeclaration(
					ParseTypeName("System.Collections.Generic.IEnumerable<NamedExample>"),
					Identifier("GetExamples")
				).AddModifiers(Token(SyntaxKind.PublicKeyword))
				.WithBody(Block(
					YieldStatement(SyntaxKind.YieldBreakStatement)
				));
		else
			yield return MethodDeclaration(
					ParseTypeName("System.Collections.Generic.IEnumerable<NamedExample>"),
					Identifier("GetExamples")
				).AddModifiers(Token(SyntaxKind.PublicKeyword))
				.WithBody(Block(
					List(
						examples.Select(exampleVariable =>
							YieldStatement(SyntaxKind.YieldReturnStatement,
								ObjectCreationExpression(ParseTypeName("NamedExample"))
									.WithArgumentList(ArgumentList(
										SeparatedList(new ExpressionSyntax[]
										{
											LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(exampleVariable.Identifier.ValueText)),
											part.ResType.IsValueType
												? ObjectCreationExpression(GenericName(Identifier("ExampleAdapter"))
														.AddTypeArgumentListArguments(IdentifierName(part.ResType.ToDisplayString())))
													.AddArgumentListArguments(Argument(IdentifierName(exampleVariable.Identifier)))
												: CastExpression(
													// explicit cast to workaround NRT warnings
													ParseTypeName("IExample<object>"),
													IdentifierName(exampleVariable.Identifier)
												)
										}.Select(Argument))
									))
							)
						)
					)
				));
		
		yield return ParseMemberDeclaration(
			$$"""
			public string SolveString(string input)
			{
				var parsedInput = {{(part.ManualInput ? "ManualInput()" : "Parse(input)")}};
				return Format(Solve(parsedInput));
			}
			"""
		)!;

		var exampleInputType = part.ManualInput ? part.InputType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) : "string";
		yield return ParseMemberDeclaration(
			$$"""
			private record Example({{exampleInputType}} Input, {{part.ResType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}} Expectation) : IExample<{{part.ResType.ToDisplayString()}}>
			{
				private string expectationFormatted = null!;
				private static {{part.PartType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}} part = new {{part.PartType.ToDisplayString()}}();
				public {{part.ResType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}} Run()
				{
					var parsedInput = {{(part.ManualInput ? "Input" : "part.Parse(Input)")}};
					return part.Solve(parsedInput);
				}
				public {{part.ResType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}} RunFormat(out string formatted)
				{
					var res = Run();
					formatted = part.Format(res);
					return res;
				}
				public string ExpectationFormatted => expectationFormatted ??= part.Format(Expectation);
			}
			"""
		)!;
	}

	private static IEnumerable<MemberDeclarationSyntax> GenerateIPartMembers(PartSource part)
	{
		yield return ParseMemberDeclaration(
			$$"""
			 {{part.ResType.ToDisplayString()}} Solve({{part.InputType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}} input);
			 """
		)!;

		if (!part.ManualInput)
			yield return ParseMemberDeclaration(
				$$"""
				  {{part.InputType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}} Parse(string input);
				  """
			)!;
		else
			yield return ParseMemberDeclaration(
				$$"""
				{{part.InputType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}} ManualInput();
				"""
			)!;
	}

	private static IEnumerable<MemberDeclarationSyntax> GeneratePartBaseMembers(PartSource part)
	{
		yield return ParseMemberDeclaration(
			$$"""
			  public virtual string Format({{part.ResType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}} res) => res!.ToString()!;
			  """
		)!;
		
		if (part is { ManualInput: false, IsStringInput: true })
		{
			yield return ParseMemberDeclaration(
				$"""
				public virtual {part.InputType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} Parse(string input) => input.Trim();
				""")!;
		}

	}
	
	private static IEnumerable<TypeDeclarationSyntax> GeneratePartDeclarations(string identifier, PartSource part)
	{
		yield return InterfaceDeclaration($"I{identifier}")
			.AddModifiers(Token(SyntaxKind.PrivateKeyword))
			.AddBaseListTypes(SimpleBaseType(IdentifierName("IPart")))
			.WithMembers(
				List(GenerateIPartMembers(part))
			);
		yield return ClassDeclaration($"{identifier}Base")
			.AddModifiers(Token(SyntaxKind.AbstractKeyword))
			.AddModifiers(part.PartClass.Modifiers.Where(m =>
				m.Kind() is SyntaxKind.PublicKeyword
					or SyntaxKind.ProtectedKeyword
					or SyntaxKind.PrivateKeyword
					or SyntaxKind.InternalKeyword
			).ToArray())
			.WithMembers(
				List(GeneratePartBaseMembers(part))
			);
		yield return ClassDeclaration(identifier)
			.AddModifiers(Token(SyntaxKind.PartialKeyword))
			.AddBaseListTypes(
				SimpleBaseType(IdentifierName($"{identifier}Base")),
				SimpleBaseType(IdentifierName($"I{identifier}"))
			).WithMembers(
				List(
					GeneratePartMembers(part)
				)
			);
	}
}