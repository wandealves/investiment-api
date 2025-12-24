using FluentAssertions;
using Investment.Application.DTOs.Auth;
using Investment.Application.Services;
using Investment.Domain.Entidades;
using Investment.Infrastructure.Repositories;
using Moq;

namespace Investment.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
        _tokenServiceMock = new Mock<ITokenService>();

        _authService = new AuthService(
            _usuarioRepositoryMock.Object,
            _tokenServiceMock.Object
        );
    }

    [Fact]
    public async Task RegisterAsync_ComDadosValidos_DeveRegistrarUsuario()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Nome = "João Silva",
            Email = "joao@teste.com",
            Senha = "Senha@123"
        };

        _usuarioRepositoryMock
            .Setup(x => x.ObterPorEmailAsync(request.Email))
            .ReturnsAsync((Usuario?)null); // Email não existe

        _usuarioRepositoryMock
            .Setup(x => x.SalvarAsync(It.IsAny<Usuario>()))
            .ReturnsAsync((Usuario u) => u);

        _tokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<Usuario>()))
            .Returns("token_jwt_mock");

        // Act
        var resultado = await _authService.RegisterAsync(request);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Token.Should().Be("token_jwt_mock");
        resultado.Data.Usuario.Should().NotBeNull();
        resultado.Data.Usuario!.Nome.Should().Be("João Silva");
        resultado.Data.Usuario.Email.Should().Be("joao@teste.com");
    }

    [Fact]
    public async Task RegisterAsync_EmailJaExiste_DeveRetornarErro()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Nome = "João Silva",
            Email = "joao@teste.com",
            Senha = "Senha@123"
        };

        var usuarioExistente = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Outro Usuário",
            Email = "joao@teste.com",
            SenhaHash = "hash",
            CriadoEm = DateTimeOffset.UtcNow
        };

        _usuarioRepositoryMock
            .Setup(x => x.ExistePorEmailAsync(request.Email))
            .ReturnsAsync(true);

        // Act
        var resultado = await _authService.RegisterAsync(request);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.ValidationErrors.Should().ContainKey("Email");
        resultado.ValidationErrors["Email"].Should().Contain("Já existe um usuário com este email");
    }

    [Fact]
    public async Task RegisterAsync_SenhaFraca_DeveRetornarErroValidacao()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Nome = "João Silva",
            Email = "joao@teste.com",
            Senha = "123456" // Senha fraca
        };

        // Act
        var resultado = await _authService.RegisterAsync(request);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.ValidationErrors.Should().ContainKey("Senha");
    }

    [Theory]
    [InlineData("abc")]             // Muito curta
    [InlineData("abcdefgh")]        // Sem número, sem maiúscula, sem especial
    [InlineData("Abcdefgh")]        // Sem número, sem especial
    [InlineData("Abcdefgh1")]       // Sem especial
    [InlineData("abcdefgh1@")]      // Sem maiúscula
    public async Task RegisterAsync_SenhasInvalidas_DeveRetornarErro(string senhaInvalida)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Nome = "João Silva",
            Email = "joao@teste.com",
            Senha = senhaInvalida
        };

        // Act
        var resultado = await _authService.RegisterAsync(request);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.ValidationErrors.Should().ContainKey("Senha");
    }

    [Fact]
    public async Task LoginAsync_ComCredenciaisValidas_DeveRetornarToken()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "joao@teste.com",
            Senha = "Senha@123"
        };

        var senhaHash = BCrypt.Net.BCrypt.HashPassword("Senha@123");
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "João Silva",
            Email = "joao@teste.com",
            SenhaHash = senhaHash,
            CriadoEm = DateTimeOffset.UtcNow
        };

        _usuarioRepositoryMock
            .Setup(x => x.ObterPorEmailAsync(request.Email))
            .ReturnsAsync(usuario);

        _tokenServiceMock
            .Setup(x => x.GenerateToken(usuario))
            .Returns("token_jwt_mock");

        // Act
        var resultado = await _authService.LoginAsync(request);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Token.Should().Be("token_jwt_mock");
        resultado.Data.Usuario.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginAsync_UsuarioNaoEncontrado_DeveRetornarErro()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "naoexiste@teste.com",
            Senha = "Senha@123"
        };

        _usuarioRepositoryMock
            .Setup(x => x.ObterPorEmailAsync(request.Email))
            .ReturnsAsync((Usuario?)null);

        // Act
        var resultado = await _authService.LoginAsync(request);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.Errors.Should().Contain("Email ou senha inválidos");
    }

    [Fact]
    public async Task LoginAsync_SenhaIncorreta_DeveRetornarErro()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "joao@teste.com",
            Senha = "SenhaErrada@123"
        };

        var senhaHash = BCrypt.Net.BCrypt.HashPassword("SenhaCorreta@123");
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "João Silva",
            Email = "joao@teste.com",
            SenhaHash = senhaHash,
            CriadoEm = DateTimeOffset.UtcNow
        };

        _usuarioRepositoryMock
            .Setup(x => x.ObterPorEmailAsync(request.Email))
            .ReturnsAsync(usuario);

        // Act
        var resultado = await _authService.LoginAsync(request);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.Errors.Should().Contain("Email ou senha inválidos");
    }

    [Fact]
    public async Task AlterarSenhaAsync_ComDadosValidos_DeveAlterarSenha()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var senhaAtual = "SenhaAntiga@123";
        var novaSenha = "SenhaNova@123";

        var senhaHash = BCrypt.Net.BCrypt.HashPassword(senhaAtual);
        var usuario = new Usuario
        {
            Id = usuarioId,
            Nome = "João Silva",
            Email = "joao@teste.com",
            SenhaHash = senhaHash,
            CriadoEm = DateTimeOffset.UtcNow
        };

        _usuarioRepositoryMock
            .Setup(x => x.ObterPorIdAsync(usuarioId))
            .ReturnsAsync(usuario);

        _usuarioRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Usuario>()))
            .ReturnsAsync((Usuario u) => u);

        // Act
        var resultado = await _authService.AlterarSenhaAsync(usuarioId, senhaAtual, novaSenha);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        _usuarioRepositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Usuario>()), Times.Once);
    }

    [Fact]
    public async Task AlterarSenhaAsync_SenhaAtualIncorreta_DeveRetornarErro()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var senhaAtual = "SenhaErrada@123";
        var novaSenha = "SenhaNova@123";

        var senhaHash = BCrypt.Net.BCrypt.HashPassword("SenhaCorreta@123");
        var usuario = new Usuario
        {
            Id = usuarioId,
            Nome = "João Silva",
            Email = "joao@teste.com",
            SenhaHash = senhaHash,
            CriadoEm = DateTimeOffset.UtcNow
        };

        _usuarioRepositoryMock
            .Setup(x => x.ObterPorIdAsync(usuarioId))
            .ReturnsAsync(usuario);

        // Act
        var resultado = await _authService.AlterarSenhaAsync(usuarioId, senhaAtual, novaSenha);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.ValidationErrors.Should().ContainKey("SenhaAtual");
        resultado.ValidationErrors["SenhaAtual"].Should().Contain("Senha atual inválida");
    }
}
