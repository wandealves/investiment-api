using Investment.Api.Endpoints;
using Investment.Application.Services;
using Investment.Infrastructure.Context;
using Investment.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<InvestmentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Registrar repositórios
builder.Services.AddScoped<IAtivoRepository, AtivoRepository>();
builder.Services.AddScoped<ICarteiraRepository, CarteiraRepository>();
builder.Services.AddScoped<ICarteiraAtivoRepository, CarteiraAtivoRepository>();
builder.Services.AddScoped<ITransacaoRepository, TransacaoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Registrar serviços
builder.Services.AddScoped<IAtivoService, AtivoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapScalarApiReference(options =>
{
    options.Title = "Investment API";
    options.Theme = ScalarTheme.BluePlanet;
    options.DarkMode = true;
});

app.UseHttpsRedirection();

// Registrar endpoints
app.RegistrarAtivoEndpoints();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}