using Hangfire.Dashboard;

namespace Investment.Api.Infrastructure;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Em desenvolvimento: permitir acesso
        if (httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
            return true;

        // Em produção: exigir autenticação
        return httpContext.User.Identity?.IsAuthenticated ?? false;
    }
}
