using mazharenko.AoCAgent.Misc;
using Spectre.Console;
using Spectre.Console.Extensions;
using Spectre.Console.Rendering;

namespace mazharenko.AoCAgent;

internal static class Renderables
{
	public static IRenderable Splash(int year, int stars = 0)
	{
		var grid = new Grid().AddColumn();
		var tree = new Tree(year, stars);
		grid.AddRow(
			new Grid().AddColumn().AddColumn()
				.AddRow(tree, new Panel(
			Align.Left(
				new FigletText($"AoC {year}")
			).MiddleAligned()
		).Height(tree.Height).NoBorder()));
			
		if (stars == 50)
			grid.AddRow(new FigletText("completed"));
		return grid;
	}

	public static IRenderable Correct(string text = "right!")
	{
		return new Grid().AddColumn().AddColumn()
			.AddRow(
				// ReSharper disable StringLiteralTypo
				new Text("""
				                 .
				                ,O,
				               ,OOO,
				         'oooooOOOOOooooo'
				           `OOOOOOOOOOO`
				             `OOOOOOO`
				             OOOO'OOOO
				            OOO'   'OOO
				           O'         'O
				         """, new Style(Color.Yellow, decoration: Decoration.Bold)),
				// ReSharper restore StringLiteralTypo
				new Panel(Align.Left(new FigletText(text)).MiddleAligned())
					.NoBorder().Height(13)
			);
	}

	public static IRenderable Incorrect(string text)
	{
		return new Grid().AddColumn().AddColumn()
			.AddRow(
				new Text("""
				          ,=*,         ,*=,
				         :####=,     ,=####:
				          '#####=   =#####'
				            '#####+#####'
				              '#######'
				              ,*#####*,
				            ,*####*####*,
				          ,*####+   +####*,
				         :####*'     '*####:
				          '*#'         '#*'
				         """, new Style(Color.Red)),
				new Panel(Align.Left(new FigletText(text)).MiddleAligned())
					.NoBorder().Height(13)
			);
	}
}