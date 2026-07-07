using SmartHome.Server.Api.Clients.Configuration;
using SmartHome.Server.Api.Controllers.Clients.Configuration;
using SmartHome.Server.Api.Controllers.Configuration;
using SmartHome.Server.Api.Controllers.Firmware.Configuration;
using SmartHome.Server.Features.Managers.Configuration;
using SmartHome.Server.Repositories.Configuration;
using SmartHome.Server.Services.Configuration;

var applicationBuilder = WebApplication.CreateBuilder(args);

applicationBuilder.Services.AddSingleton(TimeProvider.System);

ApiClientsConfiguration.ConfigureApplicationBuilder(applicationBuilder);

FeaturesManagersConfiguration.ConfigureApplicationBuilder(applicationBuilder);

RepositoriesConfiguration.ConfigureDapper();
RepositoriesConfiguration.ConfigureApplicationBuilder(applicationBuilder);

ApiControllersConfiguration.ConfigureApplicationBuilder(applicationBuilder);

ClientsApiControllersConfiguration.ConfigureApplicationBuilder(applicationBuilder);

FirmwareApiControllersConfiguration.ConfigureApplicationBuilder(applicationBuilder);

ServicesConfiguration.ConfigureApplicationBuilder(applicationBuilder);

applicationBuilder.Services.AddOpenApi();

var app = applicationBuilder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
