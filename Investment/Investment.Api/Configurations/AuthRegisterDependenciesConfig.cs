using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Investment.Api.Configurations;
public static class AuthRegisterDependenciesConfig
{
    public static void RegisterAuth(this IServiceCollection services, ConfigurationManager configuration)
    {

        // Configurar autenticação JWT (com suporte a cookies httpOnly)
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    ValidAudience = configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]!))
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

        services.AddAuthorization();
    }
}

