using Investment.Application.DTOs.Carteira;
using Investment.Application.Mappers;
using Investment.Domain.Common;
using Investment.Infrastructure.Repositories;

namespace Investment.Application.Services;

public class CarteiraService : ICarteiraService
{
    private readonly ICarteiraRepository _carteiraRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public CarteiraService(ICarteiraRepository carteiraRepository, IUsuarioRepository usuarioRepository)
    {
        _carteiraRepository = carteiraRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Result<List<CarteiraResponse>>> ObterPorUsuarioAsync(Guid usuarioId)
    {
        var carteiras = await _carteiraRepository.ObterPorUsuarioIdAsync(usuarioId);
        var responses = CarteiraMapper.ToResponseList(carteiras);
        return Result<List<CarteiraResponse>>.Success(responses);
    }

    public async Task<Result<CarteiraResponse>> ObterPorIdAsync(long id, Guid usuarioId)
    {
        // Verificar se a carteira pertence ao usuário
        var pertenceAoUsuario = await _carteiraRepository.UsuarioPossuiCarteiraAsync(usuarioId, id);
        if (!pertenceAoUsuario)
        {
            return Result<CarteiraResponse>.Failure("Acesso negado: esta carteira não pertence ao usuário autenticado");
        }

        var carteira = await _carteiraRepository.ObterPorIdAsync(id);
        if (carteira == null)
        {
            return Result<CarteiraResponse>.Failure($"Carteira com ID {id} não encontrada");
        }

        var response = CarteiraMapper.ToResponse(carteira);
        return Result<CarteiraResponse>.Success(response);
    }

    public async Task<Result<CarteiraComDetalhesResponse>> ObterComDetalhesAsync(long id, Guid usuarioId)
    {
        // Verificar ownership
        var pertenceAoUsuario = await _carteiraRepository.UsuarioPossuiCarteiraAsync(usuarioId, id);
        if (!pertenceAoUsuario)
        {
            return Result<CarteiraComDetalhesResponse>.Failure("Acesso negado: esta carteira não pertence ao usuário autenticado");
        }

        var carteira = await _carteiraRepository.ObterCompletoAsync(id);
        if (carteira == null)
        {
            return Result<CarteiraComDetalhesResponse>.Failure($"Carteira com ID {id} não encontrada");
        }

        var response = CarteiraMapper.ToResponseComDetalhes(carteira);
        return Result<CarteiraComDetalhesResponse>.Success(response);
    }

    public async Task<Result<CarteiraResponse>> CriarAsync(CarteiraRequest request, Guid usuarioId)
    {
        // Validar request
        var validationErrors = ValidarRequest(request);
        if (validationErrors.Any())
        {
            return Result<CarteiraResponse>.Failure(validationErrors);
        }

        // Verificar se usuário existe
        var usuarioExiste = await _usuarioRepository.ObterPorIdAsync(usuarioId);
        if (usuarioExiste == null)
        {
            return Result<CarteiraResponse>.Failure("Usuário não encontrado");
        }

        var carteira = CarteiraMapper.ToEntity(request, usuarioId);
        var carteiraSalva = await _carteiraRepository.SalvarAsync(carteira);
        var response = CarteiraMapper.ToResponse(carteiraSalva);
        return Result<CarteiraResponse>.Success(response);
    }

    public async Task<Result<CarteiraResponse>> AtualizarAsync(long id, CarteiraRequest request, Guid usuarioId)
    {
        // Verificar ownership
        var pertenceAoUsuario = await _carteiraRepository.UsuarioPossuiCarteiraAsync(usuarioId, id);
        if (!pertenceAoUsuario)
        {
            return Result<CarteiraResponse>.Failure("Acesso negado: esta carteira não pertence ao usuário autenticado");
        }

        // Validar request
        var validationErrors = ValidarRequest(request);
        if (validationErrors.Any())
        {
            return Result<CarteiraResponse>.Failure(validationErrors);
        }

        var carteiraExistente = await _carteiraRepository.ObterPorIdAsync(id);
        if (carteiraExistente == null)
        {
            return Result<CarteiraResponse>.Failure($"Carteira com ID {id} não encontrada");
        }

        CarteiraMapper.UpdateEntity(carteiraExistente, request);
        var carteiraAtualizada = await _carteiraRepository.AtualizarAsync(carteiraExistente);
        var response = CarteiraMapper.ToResponse(carteiraAtualizada);
        return Result<CarteiraResponse>.Success(response);
    }

    public async Task<Result> ExcluirAsync(long id, Guid usuarioId)
    {
        // Verificar ownership
        var pertenceAoUsuario = await _carteiraRepository.UsuarioPossuiCarteiraAsync(usuarioId, id);
        if (!pertenceAoUsuario)
        {
            return Result.Failure("Acesso negado: esta carteira não pertence ao usuário autenticado");
        }

        var carteiraExistente = await _carteiraRepository.ObterPorIdAsync(id);
        if (carteiraExistente == null)
        {
            return Result.Failure($"Carteira com ID {id} não encontrada");
        }

        try
        {
            var excluido = await _carteiraRepository.ExcluirAsync(id);
            if (!excluido)
            {
                return Result.Failure("Erro ao excluir a carteira");
            }

            return Result.Success();
        }
        catch
        {
            // Se houver transações, o banco impedirá a exclusão (RESTRICT)
            return Result.Failure("Não é possível excluir a carteira pois ela possui transações associadas");
        }
    }

    private Dictionary<string, List<string>> ValidarRequest(CarteiraRequest request)
    {
        var errors = new Dictionary<string, List<string>>();

        // Validar nome
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            errors.Add("Nome", new List<string> { "Nome é obrigatório" });
        }
        else if (request.Nome.Length < 3 || request.Nome.Length > 100)
        {
            errors.Add("Nome", new List<string> { "Nome deve ter entre 3 e 100 caracteres" });
        }

        // Validar descrição (opcional)
        if (!string.IsNullOrWhiteSpace(request.Descricao) && request.Descricao.Length > 500)
        {
            errors.Add("Descricao", new List<string> { "Descrição deve ter no máximo 500 caracteres" });
        }

        return errors;
    }
}
