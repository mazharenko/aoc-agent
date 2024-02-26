using System.Text.RegularExpressions;
using mazharenko.AoCAgent.Misc;
using Spectre.Console;
using Spectre.Console.Extensions;
using Spectre.Console.Rendering;

namespace mazharenko.AoCAgent;

internal static class Renderables
{
	private const string TreeTop =
		"""
		           [yellow bold]\|/
		            [underline]*[/]
		[/]
		""";
	private const string TreeDraft = 
		"""
		           >o<
		          >>O<<
		         >O<<<*<
		        >@<o<o<o<
		       >>@>>O<O<<<
		      >O<o>>>@<<<*<
		     >>@>>>@<*<<*<<<
		    >O<<<*<<<O<@<@<<<
		   >>*<<<*>>o<<<@<<*<<
		  >@<<<*>>>*>>O<<O<<o<<
		 >*>>o<*>>O>>>*>>>o>>*<<
		>O>>*>O>>O<<@>>>@<<<*<@<<
		           [grey]| |[/]
		""";

	public static Panel Tree(int starCount)
	{
		var count = 0;
		var treeString1 = Regex.Replace(TreeDraft, "[o]", m => count++ >= starCount ? $"[grey bold]{m.Value}[/]" : $"[orange1 bold]{m.Value}[/]");
		var treeString2 = Regex.Replace(treeString1, "[O]", m => count++ >= starCount ? $"[grey bold]{m.Value}[/]" : $"[blue1 bold]{m.Value}[/]");
		var treeString3 = Regex.Replace(treeString2, "[@]", m => count++ >= starCount ? $"[grey bold]{m.Value}[/]" : $"[red1 bold]{m.Value}[/]");
		var treeString = Regex.Replace(treeString3, "[*]", m => count++ >= starCount ? $"[grey bold]{m.Value}[/]" : $"[yellow1 bold]{m.Value}[/]");

		return new Panel(
			new Markup(TreeTop + treeString,
				new Style(Color.Green))
		).Height(19).Padding(2, 1).NoBorder();
	}
	
	public static IRenderable Splash(int year, int stars = 0)
	{
		var grid = new Grid().AddColumn();
		var tree = Tree(stars);
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