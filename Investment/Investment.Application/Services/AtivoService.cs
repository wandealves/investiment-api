using Gridify;
using Investment.Application.DTOs;
using Investment.Application.Mappers;
using Investment.Domain.Common;
using Investment.Infrastructure.Repositories;

namespace Investment.Application.Services;

public class AtivoService(IAtivoRepository ativoRepository) : IAtivoService
{
    public async Task<Result<AtivoResponse>> ObterPorIdAsync(long id)
    {
        var ativo = await ativoRepository.ObterPorIdAsync(id);

        if (ativo == null)
        {
            return Result<AtivoResponse>.Failure($"Ativo com ID {id} não encontrado");
        }

        var response = AtivoMapper.ToResponse(ativo);
        return Result<AtivoResponse>.Success(response);
    }

    public async Task<Result<AtivoResponse>> ObterPorCodigoAsync(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return Result<AtivoResponse>.Failure("Código é obrigatório");
        }

        var ativo = await ativoRepository.ObterPorCodigoAsync(codigo);

        if (ativo == null)
        {
            return Result<AtivoResponse>.Failure($"Ativo com código {codigo} não encontrado");
        }

        var response = AtivoMapper.ToResponse(ativo);
        return Result<AtivoResponse>.Success(response);
    }

    public async Task<Result<List<AtivoResponse>>> ObterTodosAsync()
    {
        var ativos = await ativoRepository.ObterTodosAsync();
        var responses = AtivoMapper.ToResponseList(ativos);
        return Result<List<AtivoResponse>>.Success(responses);
    }

    public async Task<Result<List<AtivoResponse>>> BuscarAsync(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
        {
            return Result<List<AtivoResponse>>.Failure("Termo de busca é obrigatório");
        }

        var ativos = await ativoRepository.BuscarAsync(termo);
        var responses = AtivoMapper.ToResponseList(ativos);
        return Result<List<AtivoResponse>>.Success(responses);
    }

    public async Task<Result<Paging<AtivoResponse>>> ObterAsync(GridifyQuery query)
    {
        var paging = await ativoRepository.ObterAsync(query);

        var responsePaging = new Paging<AtivoResponse>
        {
            Count = paging.Count,
            Data = paging.Data.Select(AtivoMapper.ToResponse).ToList()
        };

        return Result<Paging<AtivoResponse>>.Success(responsePaging);
    }

    public async Task<Result<AtivoResponse>> CriarAsync(AtivoRequest request)
    {
        var validationErrors = ValidarRequest(request);
        if (validationErrors.Any())
        {
            return Result<AtivoResponse>.Failure(validationErrors);
        }

        var codigoExiste = await ativoRepository.ExistePorCodigoAsync(request.Codigo);
        if (codigoExiste)
        {
            var errors = new Dictionary<string, List<string>>
            {
                { "Codigo", new List<string> { $"Já existe um ativo com o código {request.Codigo}" } }
            };
            return Result<AtivoResponse>.Failure(errors);
        }

        var ativo = AtivoMapper.ToEntity(request);
        var ativoSalvo = await ativoRepository.SalvarAsync(ativo);
        var response = AtivoMapper.ToResponse(ativoSalvo);
        return Result<AtivoResponse>.Success(response);
    }

    public async Task<Result<AtivoResponse>> AtualizarAsync(long id, AtivoRequest request)
    {
        var validationErrors = ValidarRequest(request);
        if (validationErrors.Any())
        {
            return Result<AtivoResponse>.Failure(validationErrors);
        }
        var ativoExistente = await ativoRepository.ObterPorIdAsync(id);
        if (ativoExistente == null)
        {
            return Result<AtivoResponse>.Failure($"Ativo com ID {id} não encontrado");
        }
        var codigoExiste = await ativoRepository.ExistePorCodigoAsync(request.Codigo, id);
        if (codigoExiste)
        {
            var errors = new Dictionary<string, List<string>>
            {
                { "Codigo", new List<string> { $"Já existe outro ativo com o código {request.Codigo}" } }
            };
            return Result<AtivoResponse>.Failure(errors);
        }
        AtivoMapper.UpdateEntity(ativoExistente, request);
        var ativoAtualizado = await ativoRepository.AtualizarAsync(ativoExistente);
        var response = AtivoMapper.ToResponse(ativoAtualizado);
        return Result<AtivoResponse>.Success(response);
    }

    public async Task<Result> ExcluirAsync(long id)
    {
        var ativoExistente = await ativoRepository.ObterPorIdAsync(id);
        if (ativoExistente == null)
        {
            return Result.Failure($"Ativo com ID {id} não encontrado");
        }
        var excluido = await ativoRepository.ExcluirAsync(id);
        if (!excluido)
        {
            return Result.Failure("Erro ao excluir o ativo");
        }
        return Result.Success();
    }

    private Dictionary<string, List<string>> ValidarRequest(AtivoRequest request)
    {
        var errors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            errors.Add("Nome", new List<string> { "Nome é obrigatório" });
        }
        else if (request.Nome.Length < 3 || request.Nome.Length > 200)
        {
            errors.Add("Nome", new List<string> { "Nome deve ter entre 3 e 200 caracteres" });
        }

        if (string.IsNullOrWhiteSpace(request.Codigo))
        {
            errors.Add("Codigo", new List<string> { "Código é obrigatório" });
        }
        else if (request.Codigo.Length < 2 || request.Codigo.Length > 20)
        {
            errors.Add("Codigo", new List<string> { "Código deve ter entre 2 e 20 caracteres" });
        }

        // Tipo é validado pelo enum - não precisa validação adicional

        if (!string.IsNullOrWhiteSpace(request.Descricao) && request.Descricao.Length > 1000)
        {
            errors.Add("Descricao", new List<string> { "Descrição deve ter no máximo 1000 caracteres" });
        }
        return errors;
    }
}