using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartHome.Server.Repositories.Abstractions;
using SmartHome.Server.Services.Processors;

namespace SmartHome.Server.Services.Configuration;

/// <summary>
/// Provides configuration methods to register assembly utilities into the host application.
/// </summary>
/// <remarks>
/// The concrete implementations of the exposed services remain internal to this assembly.
/// </remarks>
public static class ServicesConfiguration
{
    /// <summary>
    /// Registers internal assembly services and utilities into the provided application builder.
    /// </summary>
    /// <param name="applicationBuilder">
    /// The host application builder to configure.
    /// </param>
    public static void ConfigureApplicationBuilder(IHostApplicationBuilder applicationBuilder)
    {
        applicationBuilder.Services.AddHostedService(serviceProvider =>
        {
            var serviceProcessor = new HeartbeatMonitoringServiceProcessor(
                    serviceProvider.GetRequiredService<IStationsRepository>(),
                    serviceProvider.GetRequiredService<TimeProvider>(),
                    TimeSpan.FromSeconds(60),   // TODO: Move this value to some configuration file.
                    serviceProvider.GetRequiredService<ILogger<HeartbeatMonitoringServiceProcessor>>());

            return new BackgroundProcessorService(
                serviceProcessor,
                serviceProvider.GetRequiredService<TimeProvider>(),
                TimeSpan.FromSeconds(65),   // TODO: Move this value to some configuration file.
                serviceProvider.GetRequiredService<ILogger<BackgroundProcessorService>>());
        });
    }
}
