using Dapper;
using SmartHome.Server.Data;
using SmartHome.Server.Data.Converters.JsonConverters;
using SmartHome.Server.Data.Converters.TypeHandlers;
using SmartHome.Server.Data.Database;
using SmartHome.Server.Data.Repositories;
using SmartHome.Server.Managers.Factories;
using SmartHome.Server.Services;

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

// Dependency injection configuration
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(new DatabaseClient(ConnectionString));
builder.Services.AddSingleton<IDatabaseClient>(serviceProvider => serviceProvider.GetRequiredService<DatabaseClient>());
builder.Services.AddSingleton<IStationsRepository>(serviceProvider => serviceProvider.GetRequiredService<DatabaseClient>());
builder.Services.AddSingleton<ISwitchesRepository>(serviceProvider => serviceProvider.GetRequiredService<DatabaseClient>());
builder.Services.AddSingleton<ISwitchManagerFactory>(new SwitchManagerFactory());
builder.Services.AddSingleton<ITimestampProvider, TimestampProvider>();

builder.Services.AddHostedService<HeartbeatMonitoringService>();

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
