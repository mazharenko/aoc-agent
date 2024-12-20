using System.Text;
using mazharenko.AoCAgent.Generator.Mics;
using mazharenko.AoCAgent.Generator.Sources;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace mazharenko.AoCAgent.Generator;

internal partial class DaysGenerator
{
	private void GenerateEntryPoint(SourceProductionContext productionContext, ExplicitYear explicitYear)
	{
		if (!explicitYear.GenerateEntryPoint)
			return;
		productionContext.CancellationToken.ThrowIfCancellationRequested();
		productionContext.AddSource("Main.g.cs", SourceText.From(
				$$"""
				{{AutoGeneratedComment}}
				{{RestoreNullableString}}
				using Spectre.Console;
				using System.Threading.Tasks;
				using System;

				{{CodeGeneratedAttribute.AsString}}
				internal class Program
				{
					private static async Task Main(string[] args)
					{
						try
						{
							await new mazharenko.AoCAgent.Runner().Run(new {{explicitYear.Type.ToDisplayString()}}());
						}
						catch (Exception e)
						{
							AnsiConsole.WriteException(e);
							Environment.Exit(134);
						}
					}
				}
				""", Encoding.UTF8)
			);
	}
}