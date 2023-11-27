namespace mazharenko.AoCAgent;

public static class DotnetWatch
{
	public static bool IsUnderWatch()
	{
		return Environment.GetEnvironmentVariable("DOTNET_WATCH") == "1";
	}
}