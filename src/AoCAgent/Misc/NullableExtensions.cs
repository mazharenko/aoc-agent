namespace mazharenko.AoCAgent.Misc;

internal static class NullableExtensions
{
	public static T? ToNullable<T>(this T source)
		where T : struct
	{
		return source;
	}
}