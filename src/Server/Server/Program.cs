using Dapper;
using Server.Data.Database;
using Server.Data.Repositories;
using Server.Data.TypeHandlers.Dapper;

const string ConnectionString = "Server=127.0.0.1;Database=smart_home;User Id=smart_home_controller;Password=1234; Encrypt=True; TrustServerCertificate=True";

var builder = WebApplication.CreateBuilder(args);

// Dapper configuration
DefaultTypeMap.MatchNamesWithUnderscores = true;    // Enable mapping of snake case to pascal case.
SqlMapper.AddTypeHandler(new PhysicalAddressHandler());
SqlMapper.AddTypeHandler(new IPAddressHandler());

builder.Services.AddSingleton(new DatabaseClient(ConnectionString));
builder.Services.AddSingleton<IDatabaseClient>(serviceProvider => serviceProvider.GetRequiredService<DatabaseClient>());
builder.Services.AddSingleton<IStationsRepository>(serviceProvider => serviceProvider.GetRequiredService<DatabaseClient>());

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new PhysicalAddressConverter());
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
