using System.Collections;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Testing;

namespace AoCAgent.Tests.Stages;

internal class StatsStageTests : IEnumerable
{
	[TestCaseSource(typeof(StatsStageTests))]
	public async Task Should_Render_Tree(Stats stats)
	{
		var services = new ServiceCollection();
		var console = new TestConsole().EmitAnsiSequences();
		services.AddSingleton<IAnsiConsole>(console);
		new StatsStage(new RunnerContext(FakeYear.Default, services.BuildServiceProvider()))
			.RenderStats(stats);
		await Verify(console.Output).UseTextForParameters($"{stats.Stars} stars");
	}

	public IEnumerator GetEnumerator()
	{
		foreach (var stars in new []{0, 28, 50})
		{
			var stats = new Stats();
			for (var day = 1; day <= 25; day++)
			{
				if (stars / 2 >= day)
				{
					stats.Add(Day.Create(day), Part._1, true);
					stats.Add(Day.Create(day), Part._2, true);
				}
				else if ((stars + 1) / 2 >= day)
				{
					stats.Add(Day.Create(day), Part._1, true);
					stats.Add(Day.Create(day), Part._2, false);
				}
				else
				{
					stats.Add(Day.Create(day), Part._1, false);
					stats.Add(Day.Create(day), Part._2, false);
				}
			}

			yield return new TestCaseData(stats).SetArgDisplayNames($"{stats.Stars} stars stats");
		}
	}
}