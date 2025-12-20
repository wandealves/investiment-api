using System.Text;
using System.Text.Json.Serialization;
using Investment.Api.Endpoints;
using Investment.Application.Services;
using Investment.Infrastructure.Context;
using Investment.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configurar JSON para aceitar strings em enums
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddDbContext<InvestmentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Configurar autenticação JWT (com suporte a cookies httpOnly)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
        };

        // Ler token do cookie httpOnly em vez do header Authorization
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Primeiro tenta ler do cookie
                if (context.Request.Cookies.TryGetValue("access_token", out var token))
                {
                    context.Token = token;
                }
                // Fallback: Mantém compatibilidade com header Authorization (para testes/Swagger)
                else if (context.Request.Headers.ContainsKey("Authorization"))
                {
                    var authHeader = context.Request.Headers["Authorization"].ToString();
                    if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Token = authHeader.Substring("Bearer ".Length).Trim();
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

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
builder.Services.AddScoped<IAtivoRepository, AtivoRepository>();
builder.Services.AddScoped<ICarteiraRepository, CarteiraRepository>();
builder.Services.AddScoped<ICarteiraAtivoRepository, CarteiraAtivoRepository>();
builder.Services.AddScoped<ITransacaoRepository, TransacaoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Registrar serviços
builder.Services.AddScoped<IAtivoService, AtivoService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ICarteiraService, CarteiraService>();
builder.Services.AddScoped<ITransacaoService, TransacaoService>();
builder.Services.AddScoped<IPosicaoService, PosicaoService>();

// Registrar serviços de importação PDF
builder.Services.AddScoped<Investment.Application.Services.PDF.IPdfParserStrategy, Investment.Application.Services.PDF.ClearPdfParser>();
builder.Services.AddScoped<Investment.Application.Services.PDF.IPdfParserStrategy, Investment.Application.Services.PDF.XPPdfParser>();
builder.Services.AddScoped<Investment.Application.Services.PDF.IPdfParserService, Investment.Application.Services.PDF.PdfParserService>();
builder.Services.AddScoped<IImportacaoService, ImportacaoService>();

// Registrar serviços de relatórios
builder.Services.AddScoped<IRelatorioService, RelatorioService>();

// Registrar serviços de dashboard
builder.Services.AddScoped<IDashboardService, DashboardService>();

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

// Registrar endpoints
app.RegistrarAuthEndpoints();
app.RegistrarUsuarioEndpoints();
app.RegistrarCarteiraEndpoints();
app.RegistrarTransacaoEndpoints();
app.RegistrarPosicaoEndpoints();
app.RegistrarAtivoEndpoints();
app.RegistrarImportacaoEndpoints();
app.RegistrarRelatorioEndpoints();
app.RegistrarDashboardEndpoints();

app.Run();