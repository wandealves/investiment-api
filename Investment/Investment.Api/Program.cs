using Investment.Api.Configurations;
using Investment.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configurar JSON para aceitar strings em enums e usar camelCase
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

builder.Services.AddDbContext<InvestmentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.RegisterJob(builder.Configuration);

builder.Services.RegisterAuth(builder.Configuration); ;

// Configurar CORS com suporte a credentials (cookies)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            // IMPORTANTE: Em produção, especificar origem exata
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                ?? new[] { "http://localhost:3000", "http://localhost:5173" }; // Vite default port

            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials(); // CRÍTICO: Permite envio de cookies
        });
});

// Registrar repositórios
builder.Services.RegisterRepository();
// Registrar serviços
builder.Services.RegisterService();
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
// CORS deve vir antes de autenticação/autorização
app.UseCors("AllowFrontend");
// Middleware de autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();
app.UseJobConfiguration();
app.UseEndpointConfiguration();

app.Run();