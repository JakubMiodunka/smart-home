using Dapper;
using SmartHome.Server.Data.TypeHandlers;

var builder = WebApplication.CreateBuilder(args);

// Dapper configuration
DefaultTypeMap.MatchNamesWithUnderscores = true;    // Enable mapping of snake case to pascal case.
SqlMapper.AddTypeHandler(new PhysicalAddressHandler());
SqlMapper.AddTypeHandler(new IPAddressHandler());

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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
