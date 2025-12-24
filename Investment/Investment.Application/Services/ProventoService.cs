using Gridify;
using Investment.Application.DTOs.Provento;
using Investment.Application.Mappers;
using Investment.Domain.Common;
using Investment.Infrastructure.Repositories;

namespace Investment.Application.Services;

public class ProventoService : IProventoService
{
    private readonly IProventoRepository _proventoRepository;
    private readonly IAtivoRepository _ativoRepository;

    public ProventoService(
        IProventoRepository proventoRepository,
        IAtivoRepository ativoRepository)
    {
        _proventoRepository = proventoRepository;
        _ativoRepository = ativoRepository;
    }

    public async Task<Result<ProventoResponse>> ObterPorIdAsync(long id)
    {
        var provento = await _proventoRepository.ObterPorIdAsync(id);

        if (provento == null)
        {
            return Result<ProventoResponse>.Failure($"Provento com ID {id} não encontrado");
        }

        var response = ProventoMapper.ToResponse(provento);
        return Result<ProventoResponse>.Success(response);
    }

    public async Task<Result<Paging<ProventoResponse>>> ObterAsync(GridifyQuery query)
    {
        var paging = await _proventoRepository.ObterAsync(query);

        var responsePaging = new Paging<ProventoResponse>
        {
            Count = paging.Count,
            Data = paging.Data.Select(ProventoMapper.ToResponse).ToList()
        };

        return Result<Paging<ProventoResponse>>.Success(responsePaging);
    }

    public async Task<Result<List<ProventoResponse>>> ObterPorAtivoAsync(long ativoId)
    {
        // Verificar se ativo existe
        var ativo = await _ativoRepository.ObterPorIdAsync(ativoId);
        if (ativo == null)
        {
            return Result<List<ProventoResponse>>.Failure($"Ativo com ID {ativoId} não encontrado");
        }

        var proventos = await _proventoRepository.ObterPorAtivoIdAsync(ativoId);
        var responses = ProventoMapper.ToResponseList(proventos);
        return Result<List<ProventoResponse>>.Success(responses);
    }

    public async Task<Result<List<ProventoResponse>>> ObterPorPeriodoAsync(DateTimeOffset inicio, DateTimeOffset fim)
    {
        if (inicio > fim)
        {
            return Result<List<ProventoResponse>>.Failure("A data de início deve ser anterior à data de fim");
        }

        var proventos = await _proventoRepository.ObterPorPeriodoAsync(inicio, fim);
        var responses = ProventoMapper.ToResponseList(proventos);
        return Result<List<ProventoResponse>>.Success(responses);
    }

    public async Task<Result<List<ProventoResponse>>> ObterAgendadosAsync()
    {
        var proventos = await _proventoRepository.ObterAgendadosAsync();
        var responses = ProventoMapper.ToResponseList(proventos);
        return Result<List<ProventoResponse>>.Success(responses);
    }

    public async Task<Result<ProventoComTransacoesResponse>> ObterComTransacoesAsync(long id)
    {
        var provento = await _proventoRepository.ObterComDetalhesAsync(id);

        if (provento == null)
        {
            return Result<ProventoComTransacoesResponse>.Failure($"Provento com ID {id} não encontrado");
        }

        var response = ProventoMapper.ToResponseComTransacoes(provento);
        return Result<ProventoComTransacoesResponse>.Success(response);
    }

    public async Task<Result<ProventoResponse>> CriarAsync(ProventoRequest request)
    {
        // Validar request
        var validationErrors = await ValidarRequestAsync(request);
        if (validationErrors.Any())
        {
            return Result<ProventoResponse>.Failure(validationErrors);
        }

        var provento = ProventoMapper.ToEntity(request);
        var proventoSalvo = await _proventoRepository.SalvarAsync(provento);

        // Buscar provento completo com relacionamentos
        var proventoCompleto = await _proventoRepository.ObterPorIdAsync(proventoSalvo.Id);
        var response = ProventoMapper.ToResponse(proventoCompleto!);
        return Result<ProventoResponse>.Success(response);
    }

    public async Task<Result<ProventoResponse>> AtualizarAsync(long id, ProventoRequest request)
    {
        var proventoExistente = await _proventoRepository.ObterPorIdAsync(id);
        if (proventoExistente == null)
        {
            return Result<ProventoResponse>.Failure($"Provento com ID {id} não encontrado");
        }

        // Validar request
        var validationErrors = await ValidarRequestAsync(request);
        if (validationErrors.Any())
        {
            return Result<ProventoResponse>.Failure(validationErrors);
        }

        ProventoMapper.UpdateEntity(proventoExistente, request);
        var proventoAtualizado = await _proventoRepository.AtualizarAsync(proventoExistente);

        // Buscar provento completo
        var proventoCompleto = await _proventoRepository.ObterPorIdAsync(proventoAtualizado.Id);
        var response = ProventoMapper.ToResponse(proventoCompleto!);
        return Result<ProventoResponse>.Success(response);
    }

    public async Task<Result> ExcluirAsync(long id)
    {
        var proventoExistente = await _proventoRepository.ObterPorIdAsync(id);
        if (proventoExistente == null)
        {
            return Result.Failure($"Provento com ID {id} não encontrado");
        }

        var excluido = await _proventoRepository.ExcluirAsync(id);
        if (!excluido)
        {
            return Result.Failure("Erro ao excluir o provento");
        }

        return Result.Success();
    }

    public async Task<Result<ProventoResponse>> MarcarComoPagoAsync(long id)
    {
        var provento = await _proventoRepository.ObterPorIdAsync(id);
        if (provento == null)
        {
            return Result<ProventoResponse>.Failure($"Provento com ID {id} não encontrado");
        }

        if (provento.Status == StatusProvento.Cancelado)
        {
            return Result<ProventoResponse>.Failure("Não é possível marcar um provento cancelado como pago");
        }

        if (provento.Status == StatusProvento.Pago)
        {
            return Result<ProventoResponse>.Failure("Este provento já está marcado como pago");
        }

        provento.Status = StatusProvento.Pago;
        var proventoAtualizado = await _proventoRepository.AtualizarAsync(provento);

        var response = ProventoMapper.ToResponse(proventoAtualizado);
        return Result<ProventoResponse>.Success(response);
    }

    public async Task<Result<ProventoResponse>> CancelarAsync(long id)
    {
        var provento = await _proventoRepository.ObterPorIdAsync(id);
        if (provento == null)
        {
            return Result<ProventoResponse>.Failure($"Provento com ID {id} não encontrado");
        }

        if (provento.Status == StatusProvento.Pago)
        {
            return Result<ProventoResponse>.Failure("Não é possível cancelar um provento já pago");
        }

        if (provento.Status == StatusProvento.Cancelado)
        {
            return Result<ProventoResponse>.Failure("Este provento já está cancelado");
        }

        provento.Status = StatusProvento.Cancelado;
        var proventoAtualizado = await _proventoRepository.AtualizarAsync(provento);

        var response = ProventoMapper.ToResponse(proventoAtualizado);
        return Result<ProventoResponse>.Success(response);
    }

    private async Task<Dictionary<string, List<string>>> ValidarRequestAsync(ProventoRequest request)
    {
        var errors = new Dictionary<string, List<string>>();

        // Validar ativo
        if (request.AtivoId <= 0)
        {
            errors.Add("AtivoId", new List<string> { "ID do ativo inválido" });
        }
        else
        {
            var ativo = await _ativoRepository.ObterPorIdAsync(request.AtivoId);
            if (ativo == null)
            {
                errors.Add("AtivoId", new List<string> { "Ativo não encontrado" });
            }
        }

        // Validar valor por cota
        if (request.ValorPorCota <= 0)
        {
            errors.Add("ValorPorCota", new List<string> { "O valor por cota deve ser maior que zero" });
        }

        // Validar datas
        if (request.DataCom == default)
        {
            errors.Add("DataCom", new List<string> { "A data COM é obrigatória" });
        }

        if (request.DataPagamento == default)
        {
            errors.Add("DataPagamento", new List<string> { "A data de pagamento é obrigatória" });
        }

        // Validar ordem das datas se DataEx foi fornecida
        if (request.DataEx.HasValue)
        {
            if (request.DataEx.Value < request.DataCom)
            {
                errors.Add("DataEx", new List<string> { "A data EX deve ser posterior à data COM" });
            }
        }

        if (request.DataPagamento < request.DataCom)
        {
            errors.Add("DataPagamento", new List<string> { "A data de pagamento deve ser posterior à data COM" });
        }

        return errors;
    }
}
