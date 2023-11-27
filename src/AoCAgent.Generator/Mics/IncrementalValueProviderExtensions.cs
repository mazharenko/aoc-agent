using Microsoft.CodeAnalysis;

namespace mazharenko.AoCAgent.Generator.Mics;

internal static class IncrementalValueProviderExtensions
{
	public static IncrementalValuesProvider<T> WhereCombine<T>(this IncrementalValuesProvider<T> source,
		IncrementalValueProvider<bool> conditionProvider)
	{
		return source.Combine(conditionProvider)
			.Where(x => x.Right)
			.Select((x, _) => x.Left);
	}
	
	public static IncrementalValuesProvider<T> Choose<TSource, T>(this IncrementalValuesProvider<TSource> source, Func<TSource, T?> chooser)
		where T : struct
	{
		return Choose(source, (c, _) => chooser(c));
	}
	
	public static IncrementalValuesProvider<T> Choose<TSource, T>(this IncrementalValuesProvider<TSource> source, Func<TSource, CancellationToken, T?> chooser)
		where T : struct
	{
		return source.Select(chooser).Where(x => x.HasValue).Select((x, _) => x!.Value);
	}
}