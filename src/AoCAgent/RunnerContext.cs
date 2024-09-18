using mazharenko.AoCAgent.Base;
using mazharenko.AoCAgent.Client;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace mazharenko.AoCAgent;

internal class RunnerContext(YearBase year, IServiceProvider serviceCollection)
{
	public YearBase Year { get; } = year;
	public IAoCClient AoCClient { get; } = serviceCollection.GetRequiredService<IAoCClient>();
	public IAnsiConsole Console => serviceCollection.GetRequiredService<IAnsiConsole>();
	public async Task<Stats> GetCurrentStats() => await AoCClient.GetDayResults();
}