using Spectre.Console;

namespace mazharenko.AoCAgent.Misc;

internal static class RenderableExtensions
{
	public static Panel Height(this Panel panel, int? height)
	{
		panel.Height = height;
		return panel;
	}
}