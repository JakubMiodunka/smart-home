using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartHome.Server.Features.Managers.Abstractions;

namespace SmartHome.Server.Features.Managers.Configuration;

/// <summary>
/// Provides configuration methods to register assembly utilities into the host application.
/// </summary>
/// <remarks>
/// The concrete implementations of the exposed services remain internal to this assembly.
/// </remarks>
public static class FeaturesManagersConfiguration
{
    /// <summary>
    /// Registers internal assembly services and utilities into the provided application builder.
    /// </summary>
    /// <param name="applicationBuilder">
    /// The host application builder to configure.
    /// </param>
    public static void ConfigureApplicationBuilder(IHostApplicationBuilder applicationBuilder)
    {
        applicationBuilder.Services.AddSingleton<ISwitchManagerFactory, SwitchManagerFactory>();
        applicationBuilder.Services.AddSingleton<ISensorManagerFactory, SensorManagerFactory>();
    }
}
