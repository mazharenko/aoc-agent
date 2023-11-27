using Microsoft.CodeAnalysis;

namespace mazharenko.AoCAgent.Generator.Mics;

internal static class IncrementalGeneratorInitializationContextExtensions
{
	public static IncrementalValueProvider<bool> GenerateAgentProvider(this IncrementalGeneratorInitializationContext context)
	{
		return context.AnalyzerConfigOptionsProvider.Select((o, _) =>
			{
				if (o.GlobalOptions.TryGetValue("build_property.AoCAgent_GenerateAgent", out var value))
					return value.Equals("true", StringComparison.InvariantCultureIgnoreCase);
				return true;
			}
		);
	}
}