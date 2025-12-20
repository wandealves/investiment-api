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

// Configurar autenticação JWT
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
    });

builder.Services.AddAuthorization();

//// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
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
app.UseCors("AllowAll");

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