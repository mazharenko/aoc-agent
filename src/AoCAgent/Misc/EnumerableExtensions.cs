namespace mazharenko.AoCAgent.Misc;

internal static class EnumerableExtensions
{
	public static IEnumerable<TRes> Choose<T, TRes>(this IEnumerable<T> source, Func<T, TRes?> selector)
		where TRes : class
	{
		return source.Select(selector).Where(x => x is not null).Select(x => x!);
	}
}

internal static class EnumerableExtensionsStruct
{
	public static IEnumerable<TRes> Choose<T, TRes>(this IEnumerable<T> source, Func<T, TRes?> selector)
		where TRes : struct
	{
		return source.Select(selector).Where(x => x.HasValue).Select(x => x!.Value);
	}
}