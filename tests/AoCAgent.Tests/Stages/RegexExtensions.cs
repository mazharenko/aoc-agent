using System.Text.RegularExpressions;

namespace AoCAgent.Tests.Stages;

public static class RegexExtensions
{
	public static string ReplaceRegex(this string input, string regex, string replacement)
	{
		return Regex.Replace(input, regex, replacement);
	}
}