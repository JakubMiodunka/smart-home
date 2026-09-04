using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartHome.Server.Api.Clients.Abstractions;

namespace SmartHome.Server.Api.Clients.Configuration;

/// <summary>
/// Provides configuration methods to register assembly utilities into the host application.
/// </summary>
/// <remarks>
/// The concrete implementations of the exposed services remain internal to this assembly.
/// </remarks>
public static class ApiClientsConfiguration
{
    /// <summary>
    /// Registers internal assembly services and utilities into the provided application builder.
    /// </summary>
    /// <param name="applicationBuilder">
    /// The host application builder to configure.
    /// </param>
    public static void ConfigureApplicationBuilder(IHostApplicationBuilder applicationBuilder)
    {
        /*
         * Register a generic IHttpClientFactory to manage connection pooling for remote Stations.
         * Each feature is implemented in its own dedicated manager class, allowing for 
         * granular and optimized timeout policies tailored to specific operation types. 
         * This approach ensures high performance and fast-fail behavior while keeping 
         * the codebase clean, modular, and DRY.
         */
        applicationBuilder.Services.AddHttpClient();

        applicationBuilder.Services.AddSingleton<IStationApiClientFactory, StationApiClientFactory>();
    }
}
