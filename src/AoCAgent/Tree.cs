using System.Text.RegularExpressions;
using mazharenko.AoCAgent.Misc;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace mazharenko.AoCAgent;

internal class Tree : IRenderable
{
	private readonly Panel treePanel;

	private const string TreeTop =
		"""
		           
		            [grey bold underline]*
		[/]
		""";
	private const string TreeTopLit = 
		"""
		           [yellow bold]\|/
		            [underline]*[/]
		[/]
		""";
	private const string TreeEmpty = 
		"""
		           >><
		          >><<<
		         >><<<<<
		        >><<<><<<
		       >><>>><<<<<
		      >><<>>><<<<><
		     >><>>><<<<<><<<
		    >><<<><<<<<><<<<<
		   >>><<<<>>><<<<<<><<
		  >><<<>>>><>><<<><<<<<
		 >>>><<<>>>>>><>>><>><<<
		><>><>>>><<<<>>>><<<<<><<
		           [grey]| |[/]
		""";

	private const string TreeComplete = 
		"""
		           >@<
		          >>@<<
		         >@<<<@<
		        >@<@<@<@<
		       >>@>>@<@<<<
		      >@<@>>>@<<<@<
		     >>@>>>@<@<<@<<<
		    >@<<<@<<<@<@<@<<<
		   >>@<<<@>>@<<<@<<@<<
		  >@<<<@>>>@>>@<<@<<@<<
		 >@>>@<@>>@>>>@>>>@>>@<<
		>@>>@>@>>@<<@>>>@<<<@<@<<
		           [grey]| |[/]
		""";
	
	private static readonly List<int> toyIndexes = 
		Regex.Matches(TreeComplete, "[@]").Select(m => m.Index).ToList();
	
	
	
	public Tree(int year, int completeness)
	{
		var random = new Random(year);

		var candidateIndexes = new List<int>(toyIndexes);
			
		void Shuffle(IList<int> list)  
		{  
			var n = list.Count;  
			while (n > 1) {  
				n--;  
				var k = random.Next(n + 1);
				(list[k], list[n]) = (list[n], list[k]);
			} 
		}
		Shuffle(candidateIndexes);
		
		var indexesToPlaceToy = candidateIndexes.Take(completeness / 2).ToList();
		var treeArray = TreeEmpty.ToCharArray();
		var toys = new [] {'@', 'o', 'O', '*'};
		foreach (var i in indexesToPlaceToy) 
			treeArray[i] = toys[random.Next(toys.Length)];
		
		var treeString = new string(treeArray)
			.Replace("o", "[orange1 bold]o[/]")
			.Replace("O", "[blue1 bold]O[/]")
			.Replace("@", "[red1 bold]@[/]")
			.Replace("*", "[yellow1 bold]*[/]")
			;
		
		var top = completeness == 100 ? TreeTopLit : TreeTop;
		treePanel = new Panel(
			new Markup(top + treeString,
				new Style(Color.Green))
		).Height(19).Padding(2, 1).NoBorder();
	}
	
	public Measurement Measure(RenderOptions options, int maxWidth)
	{
		return ((IRenderable)treePanel).Measure(options, maxWidth);
	}

	public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
	{
		return ((IRenderable)treePanel).Render(options, maxWidth);
	}

	public int? Height => treePanel.Height;
}