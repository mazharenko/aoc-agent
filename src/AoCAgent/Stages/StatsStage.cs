using mazharenko.AoCAgent.Client;

namespace mazharenko.AoCAgent.Stages;

internal class StatsStage(RunnerContext context)
{
	public void RenderStats(Stats stats)
	{
		context.Console.Write(Renderables.Splash(context.Year.Year, stats.Stars * 100 / context.Year.MaxStars));
	}
}