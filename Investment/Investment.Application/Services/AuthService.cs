using System.Text.RegularExpressions;
using Investment.Application.DTOs.Auth;
using Investment.Domain.Common;
using Investment.Domain.Entidades;
using Investment.Infrastructure.Repositories;

namespace Investment.Application.Services;

public partial class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ITokenService _tokenService;

    public AuthService(IUsuarioRepository usuarioRepository, ITokenService tokenService)
    {
        _usuarioRepository = usuarioRepository;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        // Validar request
        var validationErrors = ValidarRegisterRequest(request);
        if (validationErrors.Any())
        {
            return Result<AuthResponse>.Failure(validationErrors);
        }

        // Verificar se email já existe
        var emailExiste = await _usuarioRepository.ExistePorEmailAsync(request.Email);
        if (emailExiste)
        {
            var errors = new Dictionary<string, List<string>>
            {
                { "Email", new List<string> { "Já existe um usuário com este email" } }
            };
            return Result<AuthResponse>.Failure(errors);
        }

        // Hash da senha
        var senhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha, workFactor: 12);

        // Criar usuario
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            SenhaHash = senhaHash,
            CriadoEm = DateTimeOffset.UtcNow
        };

        var usuarioSalvo = await _usuarioRepository.SalvarAsync(usuario);

        // Gerar token
        var token = _tokenService.GenerateToken(usuarioSalvo);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(24);

        var response = new AuthResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            Usuario = new UsuarioResponse
            {
                Id = usuarioSalvo.Id,
                Nome = usuarioSalvo.Nome,
                Email = usuarioSalvo.Email,
                CriadoEm = usuarioSalvo.CriadoEm
            }
        };

        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        // Validar request
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            var errors = new Dictionary<string, List<string>>
            {
                { "Email", new List<string> { "Email é obrigatório" } }
            };
            return Result<AuthResponse>.Failure(errors);
        }

        if (string.IsNullOrWhiteSpace(request.Senha))
        {
            var errors = new Dictionary<string, List<string>>
            {
                { "Senha", new List<string> { "Senha é obrigatória" } }
            };
            return Result<AuthResponse>.Failure(errors);
        }

        // Buscar usuario por email
        var usuario = await _usuarioRepository.ObterPorEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (usuario == null)
        {
            return Result<AuthResponse>.Failure("Email ou senha inválidos");
        }

        // Verificar senha
        var senhaValida = BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash);
        if (!senhaValida)
        {
            return Result<AuthResponse>.Failure("Email ou senha inválidos");
        }

        // Gerar token
        var token = _tokenService.GenerateToken(usuario);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(24);

        var response = new AuthResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            Usuario = new UsuarioResponse
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                CriadoEm = usuario.CriadoEm
            }
        };

        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result> AlterarSenhaAsync(Guid usuarioId, string senhaAtual, string novaSenha)
    {
        // Validar senhas
        if (string.IsNullOrWhiteSpace(senhaAtual))
        {
            var errors = new Dictionary<string, List<string>>
            {
                { "SenhaAtual", new List<string> { "Senha atual é obrigatória" } }
            };
            return Result.Failure(errors);
        }

        if (string.IsNullOrWhiteSpace(novaSenha))
        {
            var errors = new Dictionary<string, List<string>>
            {
                { "NovaSenha", new List<string> { "Nova senha é obrigatória" } }
            };
            return Result.Failure(errors);
        }

        // Validar complexidade da nova senha
        var senhaErrors = ValidarSenha(novaSenha);
        if (senhaErrors.Any())
        {
            return Result.Failure(senhaErrors);
        }

        // Buscar usuario
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
        if (usuario == null)
        {
            return Result.Failure("Usuário não encontrado");
        }

        // Verificar senha atual
        var senhaAtualValida = BCrypt.Net.BCrypt.Verify(senhaAtual, usuario.SenhaHash);
        if (!senhaAtualValida)
        {
            var errors = new Dictionary<string, List<string>>
            {
                { "SenhaAtual", new List<string> { "Senha atual inválida" } }
            };
            return Result.Failure(errors);
        }

        // Hash nova senha
        usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(novaSenha, workFactor: 12);

        await _usuarioRepository.AtualizarAsync(usuario);

        return Result.Success();
    }

    private Dictionary<string, List<string>> ValidarRegisterRequest(RegisterRequest request)
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

        // Validar senha
        var senhaErrors = ValidarSenha(request.Senha);
        if (senhaErrors.Any())
        {
            errors = errors.Concat(senhaErrors).ToDictionary(x => x.Key, x => x.Value);
        }

        return errors;
    }

    private Dictionary<string, List<string>> ValidarSenha(string senha)
    {
        var errors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(senha))
        {
            errors.Add("Senha", new List<string> { "Senha é obrigatória" });
            return errors;
        }

        var senhaErros = new List<string>();

        if (senha.Length < 8)
        {
            senhaErros.Add("A senha deve ter no mínimo 8 caracteres");
        }

        if (!senha.Any(char.IsUpper))
        {
            senhaErros.Add("A senha deve conter pelo menos uma letra maiúscula");
        }

        if (!senha.Any(char.IsLower))
        {
            senhaErros.Add("A senha deve conter pelo menos uma letra minúscula");
        }

        if (!senha.Any(char.IsDigit))
        {
            senhaErros.Add("A senha deve conter pelo menos um dígito");
        }

        if (!senha.Any(c => !char.IsLetterOrDigit(c)))
        {
            senhaErros.Add("A senha deve conter pelo menos um caractere especial");
        }

        if (senhaErros.Any())
        {
            errors.Add("Senha", senhaErros);
        }

        return errors;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
