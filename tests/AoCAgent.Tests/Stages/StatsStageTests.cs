using System.Collections;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Testing;

namespace AoCAgent.Tests.Stages;

internal class StatsStageTests
{
	[TestCaseSource(nameof(Before2025Cases))]
	[TestCaseSource(nameof(After2025Cases))]
	public async Task Should_Render_Tree(YearBase yearBase, Stats stats)
	{
		var services = new ServiceCollection();
		var console = new TestConsole().EmitAnsiSequences();
		services.AddSingleton<IAnsiConsole>(console);
		new StatsStage(new RunnerContext(yearBase, services.BuildServiceProvider()))
			.RenderStats(stats);
		await Verify(console.Output).UseTextForParameters($"year {yearBase.Year}_{stats.Stars} stars");
	}

	public static IEnumerable Before2025Cases()
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
			{
				var yearBase = A.Fake<YearBase>();
				A.CallTo(() => yearBase.Year).Returns(year);
				A.CallTo(() => yearBase.MaxStars).Returns(50);
				yield return new TestCaseData(yearBase, stats)
					.SetArgDisplayNames(year.ToString(), $"{stats.Stars} stars stats");
			}
		}
	}
	
	public static IEnumerable After2025Cases()
	{
		foreach (var solvedDays in new[] { 0, 6, 12 })
		{
			var stats = new Stats();
			for (var day = 1; day <= 12; day++)
			{
				if (day <= solvedDays)
				{
					stats.Solved(DayNum.Create(day), PartNum._1);
					stats.Solved(DayNum.Create(day), PartNum._2);
				}
			}

			foreach (var year in new[] { 2025 })
			{
				var yearBase = A.Fake<YearBase>();
				A.CallTo(() => yearBase.Year).Returns(year);
				A.CallTo(() => yearBase.MaxStars).Returns(24);
				yield return new TestCaseData(yearBase, stats)
					.SetArgDisplayNames(year.ToString(), $"{stats.Stars} stars stats");
			}
		}
	}
}