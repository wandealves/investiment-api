using Investment.Domain.Entidades;

namespace Investment.Infrastructure.Repositories;

public interface ICalculoIRRepository
{
    Task<CalculoIR?> ObterPorIdAsync(Guid id);
    Task<List<CalculoIR>> ObterPorUsuarioIdAsync(Guid usuarioId);
    Task<CalculoIR?> ObterUltimoPorUsuarioEAnoAsync(Guid usuarioId, int? ano);
    Task SalvarAsync(CalculoIR calculoIR);
    Task ExcluirAsync(Guid id);
}
