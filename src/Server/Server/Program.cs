using Dapper;
using SmartHome.Server.Data.Converters.JsonConverters;
using SmartHome.Server.Data.Converters.TypeHandlers;
using SmartHome.Server.Data.Database;
using SmartHome.Server.Data.Repositories;
using SmartHome.Server.Managers.Factories;
using SmartHome.Server.Services;
using SmartHome.Server.Services.Processors;

const string ConnectionString = "Server=127.0.0.1;Database=smart_home;User Id=smart_home_controller;Password=1234; Encrypt=True; TrustServerCertificate=True";

var builder = WebApplication.CreateBuilder(args);

// Dapper configuration
DefaultTypeMap.MatchNamesWithUnderscores = true;    // Enable mapping of snake case to pascal case.
SqlMapper.AddTypeHandler(new PhysicalAddressHandler());
SqlMapper.AddTypeHandler(new IPAddressHandler());

// JSON serializer configuration
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new PhysicalAddressConverter());
});

// TODO: Clean DI configuration.
// Dependency injection configuration
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHttpContextAccessor();

/*
 * Register a generic IHttpClientFactory to manage connection pooling for remote Stations.
 * Each Station feature is implemented in its own dedicated manager class, allowing for 
 * granular and optimized timeout policies tailored to specific operation types. 
 * This approach ensures high performance and fast-fail behavior while keeping 
 * the codebase clean, modular, and DRY.
 */
builder.Services.AddHttpClient();

builder.Services.AddSingleton(new DatabaseClient(ConnectionString));
builder.Services.AddSingleton<IDatabaseClient>(serviceProvider => serviceProvider.GetRequiredService<DatabaseClient>());
builder.Services.AddSingleton<IStationsRepository>(serviceProvider => serviceProvider.GetRequiredService<DatabaseClient>());
builder.Services.AddSingleton<ISwitchesRepository>(serviceProvider => serviceProvider.GetRequiredService<DatabaseClient>());
builder.Services.AddSingleton<ISwitchManagerFactory>(serviceProvider => new SwitchManagerFactory(
    serviceProvider.GetRequiredService<IHttpClientFactory>(),
    serviceProvider.GetRequiredService<IStationsRepository>(),
    serviceProvider.GetRequiredService<ISwitchesRepository>()));

builder.Services.AddHostedService(serviceProvider =>
{  
    var serviceProcessor = new HeartbeatMonitoringServiceProcessor(
            serviceProvider.GetRequiredService<IStationsRepository>(),
            serviceProvider.GetRequiredService<ISwitchesRepository>(),
            serviceProvider.GetRequiredService<TimeProvider>(),
            TimeSpan.FromSeconds(60),   // TODO: Move this value to some configuration file.
            serviceProvider.GetRequiredService<ILogger<HeartbeatMonitoringServiceProcessor>>());

    return new BackgroundProcessorService(
        serviceProcessor,
        serviceProvider.GetRequiredService<TimeProvider>(),
        TimeSpan.FromSeconds(65),   // TODO: Move this value to some configuration file.
        serviceProvider.GetRequiredService<ILogger<BackgroundProcessorService>>());
});

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
