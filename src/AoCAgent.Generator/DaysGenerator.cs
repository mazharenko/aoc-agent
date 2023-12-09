using System.Collections.Immutable;
using mazharenko.AoCAgent.Generator.Mics;
using mazharenko.AoCAgent.Generator.Sources;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace mazharenko.AoCAgent.Generator;

[Generator(LanguageNames.CSharp)]
internal partial class DaysGenerator : IIncrementalGenerator
{
	private static int? TryGetYear(string identifier)
	{
		if (identifier.StartsWith("Year") && identifier.Length == 8)
			if (int.TryParse(identifier.Substring(4), out var num))
				return num;
		return null;
	}
	
	private static int? TryGetDay(string identifier)
	{
		if (identifier.StartsWith("Day") && identifier.Length == 5)
			if (int.TryParse(identifier.Substring(3), out var num))
				return num;
		return null;
	}

	private static bool HasAttribute(GeneratorSyntaxContext c, INamedTypeSymbol partType, ISymbol dayType, Type attributeType)
	{
		var attributeSymbol = c.SemanticModel.Compilation.GetTypeByMetadataName(attributeType.FullName!)!;
		return partType.GetAttributes()
				.Any(a => SymbolEqualityComparer.Default.Equals(attributeSymbol, a.AttributeClass))
			|| dayType.GetAttributes()
				.Any(a => SymbolEqualityComparer.Default.Equals(attributeSymbol, a.AttributeClass));
	}


	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var generateAgentProvider = context.GenerateAgentProvider();
		
		var generateEntryPointProvider =
			context.SyntaxProvider.ForAttributeWithMetadataName(
					typeof(GenerateEntryPointAttribute).FullName!,
					(_, _) => true,
					(c, _) => c.TargetSymbol
				).WithComparer(SymbolEqualityComparer.Default)
				.WhereCombine(generateAgentProvider).Collect();
		var explicitYearsProvider =
			context.SyntaxProvider.CreateSyntaxProvider(
					(n, _) => n is ClassDeclarationSyntax c
					          && TryGetYear(c.Identifier.ValueText).HasValue
					          && c.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)),
					(c, ctoken) =>
					{
						ctoken.ThrowIfCancellationRequested();
						var syntax = (ClassDeclarationSyntax)c.Node;
						var symbol = c.SemanticModel.GetDeclaredSymbol(syntax)!;
		
						return new ExplicitYear(
							TryGetYear(syntax.Identifier.ValueText)!.Value,
							syntax,
							symbol,
							false
						);
					}
				).WhereCombine(generateAgentProvider)
				.Combine(generateEntryPointProvider)
				.Select((x, _) =>
				{
					var (explicitYear, generateEntryPointClasses) = x;
					var generateEntryPoint =
						generateEntryPointClasses.Any(symbol => SymbolEqualityComparer.Default.Equals(symbol, explicitYear.Type));
					return explicitYear with { GenerateEntryPoint = generateEntryPoint };
				}).Collect();
		
		var singleExplicitYearProvider =
			explicitYearsProvider
				.Select((years, _) =>
				{
					if (years.Length == 1)
						return years[0];
					return (ExplicitYear?)null;
				});
		
		var daysProvider =
			context.SyntaxProvider.CreateSyntaxProvider(
					(n, _) => n is ClassDeclarationSyntax c
					          && TryGetDay(c.Identifier.ValueText).HasValue
					          && c.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)),
					(c, ctoken) =>
					{
						ctoken.ThrowIfCancellationRequested();
		
						var name = ((ClassDeclarationSyntax)c.Node).Identifier.ValueText;

						var part1 = GetPart(c, "Part1");
						var part2 = GetPart(c, "Part2");
						if (part1 is null || part2 is null)
							return (DaySource?)null;
		
						var yearNumFromNamespace =
							c.Node.Ancestors()
								.OfType<BaseNamespaceDeclarationSyntax>()
								.SelectMany(ns => ns.Name.ToString().Split('.'))
								.Select(TryGetYear)
								.FirstOrDefault(yearNum => yearNum.HasValue);

						return new DaySource(
							TryGetDay(name)!.Value,
							yearNumFromNamespace,
							(ClassDeclarationSyntax)c.Node,
							part1.Value,
							part2.Value
						);

						static PartSource? GetPart(GeneratorSyntaxContext c, string name)
						{
							var partClass = ((ClassDeclarationSyntax)c.Node).Members
								.OfType<ClassDeclarationSyntax>()
								.FirstOrDefault(part => part.Identifier.ValueText == name);
							if (partClass is null) return null;
							var partType = c.SemanticModel.GetDeclaredSymbol(partClass)!;
							var dayType = c.SemanticModel.GetDeclaredSymbol(c.Node)!;
							var solve = partType.GetMembers("Solve").OfType<IMethodSymbol>().FirstOrDefault();

							var stringType = c.SemanticModel.Compilation.GetSpecialType(SpecialType.System_String);

							var skipNoExamples = HasAttribute(c, partType, dayType, typeof(BypassNoExamplesAttribute));
							var manualInput = HasAttribute(c, partType, dayType, typeof(ManualInputAttribute));
							var manualInterpretation = HasAttribute(c, partType, dayType, typeof(ManualInterpretationAttribute));

							if (solve is null)
								return new PartSource
								{
									PartClass = partClass,
									PartType = partType,
									InputType = stringType,
									IsStringInput = true,
									ResType = stringType,
									IsStringRes = true,
									BypassNoExamples = skipNoExamples,
									ManualInput = manualInput,
									ManualInterpretation = manualInterpretation
								};
							var resType = solve.ReturnType;
							var solveParameter = solve.Parameters.FirstOrDefault();
							if (solveParameter is null)
								return new PartSource
								{
									PartClass = partClass,
									PartType = partType,
									InputType = stringType,
									IsStringInput = true,
									ResType = resType,
									IsStringRes = SymbolEqualityComparer.Default.Equals(stringType, resType),
									BypassNoExamples = skipNoExamples,
									ManualInput = manualInput,
									ManualInterpretation = manualInterpretation
								};
							return new PartSource
							{
								PartClass = partClass,
								PartType = partType,
								InputType = solveParameter.Type,
								IsStringInput = SymbolEqualityComparer.Default.Equals(stringType, solveParameter.Type),
								ResType = resType,
								IsStringRes = SymbolEqualityComparer.Default.Equals(stringType, resType),
								BypassNoExamples = skipNoExamples,
								ManualInput = manualInput,
								ManualInterpretation = manualInterpretation
							};
						}
					}
				).Choose(d => d)
				.WhereCombine(generateAgentProvider);
		
		var dayWithResolvedYearProvider =
			daysProvider
				.Combine(singleExplicitYearProvider)
				.Choose(x =>
				{
					var (day, explicitYear) = x;
					if (day.Year.HasValue)
						return day;
					if (explicitYear is not null)
						return day with { Year = explicitYear.Value.Number };
					return (DaySource?)null;
				});
		
		context.RegisterSourceOutput(singleExplicitYearProvider, (productionContext, year) =>
		{
			if (year is not null)
				GenerateEntryPoint(productionContext, year.Value);
		});
		
		context.RegisterSourceOutput(explicitYearsProvider, (productionContext, explicitYears) =>
		{
			if (explicitYears.Length > 1)
				productionContext.ReportDiagnostic(Diagnostic.Create(Diagnostics.MoreThatOneExplicitYear, null));
		});
		
		context.RegisterSourceOutput(daysProvider.Combine(singleExplicitYearProvider), (productionContext, x) =>
		{
			var (day, explicitYear) = x;
			if (day.Year is null && explicitYear is null)
				productionContext.ReportDiagnostic(Diagnostic.Create(Diagnostics.NoExplicitYear, day.Syntax.GetLocation()));
		});
		
		context.RegisterSourceOutput(dayWithResolvedYearProvider, GenerateBaseTypes);
		
		var yearsProvider =
			dayWithResolvedYearProvider
				.Collect()
				.Combine(singleExplicitYearProvider)
				.SelectMany((x, _) =>
					{
						var (days, explicitYear) = x;
						return days.GroupBy(y => y.Year!.Value, y => y,
							(yearNum, yearDays) =>
							{
								var yearClass = explicitYear?.Number == yearNum
									? new YearClass(yearNum, $"Year{yearNum}", explicitYear.Value.Syntax.GetContainingNamespace()?.ToString())
									: new YearClass(yearNum, $"Year{yearNum}", "mazharenko.AoCAgent.GeneratedAgent");
								return new YearSource(yearClass, yearDays.ToImmutableArray());
							}
						);
					}
				);
		
		context.RegisterSourceOutput(yearsProvider, GenerateYear);
		context.RegisterSourceOutput(yearsProvider.Select((x, _) => x.YearClass).Collect().Combine(generateAgentProvider), (productionContext, x) =>
		{
			var (years, generateAgent) = x;
			if (!generateAgent)
				return;
			GenerateYearCollection(productionContext, years);
		});
	}


}