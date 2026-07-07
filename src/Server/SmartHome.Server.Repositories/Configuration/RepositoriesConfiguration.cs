using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartHome.Server.Repositories.Abstractions;
using SmartHome.Server.Repositories.TypeHandlers;

namespace SmartHome.Server.Repositories.Configuration;

/// <summary>
/// Provides configuration methods to register assembly utilities into the host application.
/// </summary>
/// <remarks>
/// The concrete implementations of the exposed services remain internal to this assembly.
/// </remarks>
public static class RepositoriesConfiguration
{
    private const string ConnectionString = "Server=127.0.0.1;Database=smart_home;User Id=smart_home_app_user;Password=1234; Encrypt=True; TrustServerCertificate=True"; // TODO: Provide connection string to SQL server instance here.

    /// <summary>
    /// Registers internal assembly services and utilities into the provided application builder.
    /// </summary>
    /// <param name="applicationBuilder">
    /// The host application builder to configure.
    /// </param>
    public static void ConfigureApplicationBuilder(IHostApplicationBuilder applicationBuilder)
    {
        applicationBuilder.Services.AddSingleton(new DatabaseClient(ConnectionString));
        applicationBuilder.Services.AddSingleton<IStationsRepository>(serviceProvider => serviceProvider.GetRequiredService<DatabaseClient>());
        applicationBuilder.Services.AddSingleton<ISwitchesRepository>(serviceProvider => serviceProvider.GetRequiredService<DatabaseClient>());
    }

    /// <summary>
    /// Configures Dapper utilities.
    /// </summary>
    public static void ConfigureDapper()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;    // Enable mapping of snake case to pascal case.

        SqlMapper.AddTypeHandler(new PhysicalAddressHandler());
        SqlMapper.AddTypeHandler(new IPAddressHandler());
    }
}
