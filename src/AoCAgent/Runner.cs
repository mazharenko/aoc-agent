using System.Diagnostics;
using JetBrains.Annotations;
using mazharenko.AoCAgent.Base;
using mazharenko.AoCAgent.Client;
using mazharenko.AoCAgent.Stages;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace mazharenko.AoCAgent;

[PublicAPI]
public class Runner
{
	// todo
	private static string GetSessionKey()
	{
		const string sessionCookieFileName = "SESSION.COOKIE";

		var fileExists = File.Exists(sessionCookieFileName);
		var existingSessionValue =
			fileExists ? File.ReadAllText(sessionCookieFileName) : "";

		switch (fileExists, existingSessionValue, DotnetWatch.IsUnderWatch())
		{
			case (false, _, false):
			case (true, "", false):
				var newSessionValue = AnsiConsole.Prompt(new TextPrompt<string>("Session key was not found. Please provide its value:").Secret());
				File.WriteAllText(sessionCookieFileName, newSessionValue);
				return newSessionValue;
			case (false, _, true):
				File.WriteAllText(sessionCookieFileName, "");
				throw new Exception("Session key was not found. Empty file is created");
			case (true, "", true):
				throw new Exception("Session key value was not found");
			case (true, var foundValue, _):
				return foundValue.Trim();
		}
	}

	public async Task Run(YearBase year)
	{
		var services =
				new ServiceCollection()
					.AddSingleton(year)
					.AddSingleton<IAoCClient>(serviceProvider =>
						new AoCCachingClient(year.Year,
							new AoCClient(year.Year, GetSessionKey(), serviceProvider.GetRequiredService<IHttpClientFactory>())))
					.AddSingleton<IAnsiConsole>(_ => AnsiConsole.Create(new AnsiConsoleSettings()))
					.AddSingleton<RunnerContext>()
					.AddSingleton<RunnerSequence>()
					.AddSingleton<StatsStage>()
					.AddSingleton<CheckExamplesStage>()
					.AddSingleton<ICheckPartExamplesSubStage, CheckPartExamplesSubStage>()
					.AddSingleton<SubmitAnswerSubStage>()
					.AddSingleton<FailedExamplesStage>()
					.AddSingleton<SubmitAnswersStage>()
					.AddHttpClient()
			;
		await using var serviceProvider = services.BuildServiceProvider();
		await serviceProvider.GetRequiredService<RunnerSequence>().Run();
	}
}