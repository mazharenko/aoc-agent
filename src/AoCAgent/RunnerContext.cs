using mazharenko.AoCAgent.Base;
using mazharenko.AoCAgent.Client;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace mazharenko.AoCAgent;

internal class RunnerContext(YearBase year, IServiceProvider serviceProvider)
{
	public YearBase Year { get; } = year;
	public IAoCClient AoCClient => serviceProvider.GetRequiredService<IAoCClient>();
	public IAnsiConsole Console => serviceProvider.GetRequiredService<IAnsiConsole>();
	public async Task<Stats> GetCurrentStats() => await AoCClient.GetDayResults();
}