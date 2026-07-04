var builder = WebApplication.CreateBuilder(args);

// Dependency injection configuration
builder.Services.AddSingleton(TimeProvider.System);



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
