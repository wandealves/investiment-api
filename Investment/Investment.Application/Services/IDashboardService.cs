using Investment.Application.DTOs.Dashboard;
using Investment.Domain.Common;

namespace Investment.Application.Services;

public interface IDashboardService
{
    Task<Result<DashboardMetricsResponse>> ObterMetricasAsync(Guid usuarioId);
    Task<Result<List<AlocacaoResponse>>> ObterAlocacaoAsync(Guid usuarioId);
    Task<Result<List<EvolucaoPatrimonioResponse>>> ObterEvolucaoPatrimonialAsync(Guid usuarioId);
}
