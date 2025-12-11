using System.Text.RegularExpressions;
using Investment.Application.DTOs.Usuario;
using Investment.Application.Mappers;
using Investment.Domain.Common;
using Investment.Infrastructure.Repositories;

namespace Investment.Application.Services;

public partial class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public UsuarioService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Result<UsuarioResponse>> ObterPorIdAsync(Guid id, Guid usuarioAutenticadoId)
    {
        // Verificar autorização - usuário só pode acessar seus próprios dados
        if (id != usuarioAutenticadoId)
        {
            return Result<UsuarioResponse>.Failure("Acesso negado: você só pode acessar seus próprios dados");
        }

        var usuario = await _usuarioRepository.ObterPorIdAsync(id);

        if (usuario == null)
        {
            return Result<UsuarioResponse>.Failure($"Usuário com ID {id} não encontrado");
        }

        var response = UsuarioMapper.ToResponse(usuario);
        return Result<UsuarioResponse>.Success(response);
    }

    public async Task<Result<UsuarioComCarteirasResponse>> ObterComCarteirasAsync(Guid id, Guid usuarioAutenticadoId)
    {
        // Verificar autorização
        if (id != usuarioAutenticadoId)
        {
            return Result<UsuarioComCarteirasResponse>.Failure("Acesso negado: você só pode acessar seus próprios dados");
        }

        var usuario = await _usuarioRepository.ObterComCarteirasAsync(id);

        if (usuario == null)
        {
            return Result<UsuarioComCarteirasResponse>.Failure($"Usuário com ID {id} não encontrado");
        }

        var response = UsuarioMapper.ToResponseComCarteiras(usuario);
        return Result<UsuarioComCarteirasResponse>.Success(response);
    }

    public async Task<Result<UsuarioResponse>> AtualizarAsync(Guid id, UsuarioRequest request, Guid usuarioAutenticadoId)
    {
        // Verificar autorização
        if (id != usuarioAutenticadoId)
        {
            return Result<UsuarioResponse>.Failure("Acesso negado: você só pode atualizar seus próprios dados");
        }

        // Validar request
        var validationErrors = ValidarRequest(request);
        if (validationErrors.Any())
        {
            return Result<UsuarioResponse>.Failure(validationErrors);
        }

        var usuarioExistente = await _usuarioRepository.ObterPorIdAsync(id);
        if (usuarioExistente == null)
        {
            return Result<UsuarioResponse>.Failure($"Usuário com ID {id} não encontrado");
        }

        // Verificar se email já existe (excluindo o próprio usuário)
        var emailExiste = await _usuarioRepository.ExistePorEmailAsync(request.Email, id);
        if (emailExiste)
        {
            var errors = new Dictionary<string, List<string>>
            {
                { "Email", new List<string> { $"Já existe outro usuário com o email {request.Email}" } }
            };
            return Result<UsuarioResponse>.Failure(errors);
        }

        UsuarioMapper.UpdateEntity(usuarioExistente, request);
        var usuarioAtualizado = await _usuarioRepository.AtualizarAsync(usuarioExistente);
        var response = UsuarioMapper.ToResponse(usuarioAtualizado);
        return Result<UsuarioResponse>.Success(response);
    }

    public async Task<Result> ExcluirAsync(Guid id, Guid usuarioAutenticadoId)
    {
        // Verificar autorização
        if (id != usuarioAutenticadoId)
        {
            return Result.Failure("Acesso negado: você só pode excluir sua própria conta");
        }

        var usuarioExistente = await _usuarioRepository.ObterPorIdAsync(id);
        if (usuarioExistente == null)
        {
            return Result.Failure($"Usuário com ID {id} não encontrado");
        }

        var excluido = await _usuarioRepository.ExcluirAsync(id);
        if (!excluido)
        {
            return Result.Failure("Erro ao excluir o usuário");
        }

        return Result.Success();
    }

    private Dictionary<string, List<string>> ValidarRequest(UsuarioRequest request)
    {
        var errors = new Dictionary<string, List<string>>();

        // Validar nome
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            errors.Add("Nome", new List<string> { "Nome é obrigatório" });
        }
        else if (request.Nome.Length < 3 || request.Nome.Length > 150)
        {
            errors.Add("Nome", new List<string> { "Nome deve ter entre 3 e 150 caracteres" });
        }

        // Validar email
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add("Email", new List<string> { "Email é obrigatório" });
        }
        else if (request.Email.Length > 200)
        {
            errors.Add("Email", new List<string> { "Email deve ter no máximo 200 caracteres" });
        }
        else if (!EmailRegex().IsMatch(request.Email))
        {
            errors.Add("Email", new List<string> { "Email inválido" });
        }

        return errors;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
