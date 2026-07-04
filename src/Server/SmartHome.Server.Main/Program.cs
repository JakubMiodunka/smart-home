
// using Dapper;
// using SmartHome.Server.ApiClients.StationApi;
// using SmartHome.Server.Data.Converters.JsonConverters;
// using SmartHome.Server.Data.Converters.TypeHandlers;
// using SmartHome.Server.Data.Database;
// using SmartHome.Server.Data.Repositories;
// using SmartHome.Server.Managers.Factories;
// using SmartHome.Server.Services;
// using SmartHome.Server.Services.Processors;


var builder = WebApplication.CreateBuilder(args);

// Dependency injection configuration
builder.Services.AddSingleton(TimeProvider.System);


// 
// 
// 
// builder.Services.AddHostedService(serviceProvider =>
// {
//     var serviceProcessor = new HeartbeatMonitoringServiceProcessor(
//             serviceProvider.GetRequiredService<IStationsRepository>(),
//             serviceProvider.GetRequiredService<TimeProvider>(),
//             TimeSpan.FromSeconds(60),   // TODO: Move this value to some configuration file.
//             serviceProvider.GetRequiredService<ILogger<HeartbeatMonitoringServiceProcessor>>());
// 
//     return new BackgroundProcessorService(
//         serviceProcessor,
//         serviceProvider.GetRequiredService<TimeProvider>(),
//         TimeSpan.FromSeconds(65),   // TODO: Move this value to some configuration file.
//         serviceProvider.GetRequiredService<ILogger<BackgroundProcessorService>>());
// });

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
