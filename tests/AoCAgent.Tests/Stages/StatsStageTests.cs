using System.Collections;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Testing;

namespace AoCAgent.Tests.Stages;

internal class StatsStageTests : IEnumerable
{
	[TestCaseSource(typeof(StatsStageTests))]
	public async Task Should_Render_Tree(int year, Stats stats)
	{
		var services = new ServiceCollection();
		var console = new TestConsole().EmitAnsiSequences();
		services.AddSingleton<IAnsiConsole>(console);
		var yearBase = A.Fake<YearBase>();
		A.CallTo(() => yearBase.Year).Returns(year);
		new StatsStage(new RunnerContext(yearBase, services.BuildServiceProvider()))
			.RenderStats(stats);
		await Verify(console.Output).UseTextForParameters($"year {year}_{stats.Stars} stars");
	}

	public IEnumerator GetEnumerator()
	{
		foreach (var solvedDays in new[] { 0, 14, 25 })
		{
			var stats = new Stats();
			for (var day = 1; day <= 25; day++)
			{
				if (day <= solvedDays)
				{
					stats.Solved(DayNum.Create(day), PartNum._1);
					stats.Solved(DayNum.Create(day), PartNum._2);
				}
			}

			foreach (var year in new[] { 2020, 2024 })
				yield return new TestCaseData(year, stats)
					.SetArgDisplayNames(year.ToString(), $"{stats.Stars} stars stats");
		}
	}
}