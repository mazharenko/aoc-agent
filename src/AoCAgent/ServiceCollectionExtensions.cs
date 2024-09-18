using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace mazharenko.AoCAgent;

[PublicAPI]
public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddRunner(this IServiceCollection services)
	{
		services.AddSingleton<Runner>();
		return services;
	}
}