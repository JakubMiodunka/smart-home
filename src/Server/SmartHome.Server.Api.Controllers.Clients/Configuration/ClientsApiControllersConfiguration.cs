using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SmartHome.Server.Api.Controllers.Clients.Configuration;

/// <summary>
/// Provides configuration methods to register assembly utilities into the host application.
/// </summary>
/// <remarks>
/// The concrete implementations of the exposed services remain internal to this assembly.
/// </remarks>
public static class ClientsApiControllersConfiguration
{
    /// <summary>
    /// Registers internal assembly services and utilities into the provided application builder.
    /// </summary>
    /// <param name="applicationBuilder">
    /// The host application builder to configure.
    /// </param>
    public static void ConfigureApplicationBuilder(IHostApplicationBuilder applicationBuilder)
    {
        applicationBuilder.Services
            .AddControllers()
            .AddApplicationPart(typeof(ClientsApiControllersConfiguration).Assembly);
    }
}
