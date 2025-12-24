using FluentAssertions;
using Investment.Application.DTOs.Carteira;
using Investment.Application.DTOs.Posicao;
using Investment.Application.Services;
using Investment.Domain.Common;
using Investment.Domain.Entidades;
using Investment.Infrastructure.Repositories;
using Moq;
using Gridify;

namespace Investment.Tests.Services;

public class CarteiraServiceTests
{
    private readonly Mock<ICarteiraRepository> _carteiraRepositoryMock;
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly Mock<IPosicaoService> _posicaoServiceMock;
    private readonly Mock<ITransacaoRepository> _transacaoRepositoryMock;
    private readonly CarteiraService _carteiraService;

    public CarteiraServiceTests()
    {
        _carteiraRepositoryMock = new Mock<ICarteiraRepository>();
        _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
        _posicaoServiceMock = new Mock<IPosicaoService>();
        _transacaoRepositoryMock = new Mock<ITransacaoRepository>();

        _carteiraService = new CarteiraService(
            _carteiraRepositoryMock.Object,
            _usuarioRepositoryMock.Object,
            _posicaoServiceMock.Object,
            _transacaoRepositoryMock.Object
        );
    }

    [Fact]
    public async Task ObterPorIdAsync_QuandoCarteiraExiste_DeveRetornarCarteira()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var carteiraId = 1L;
        var carteira = new Carteira
        {
            Id = carteiraId,
            UsuarioId = usuarioId,
            Nome = "Carteira Teste",
            Descricao = "Descrição teste",
            CriadaEm = DateTimeOffset.UtcNow
        };

        _carteiraRepositoryMock
            .Setup(x => x.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId))
            .ReturnsAsync(true);

        _carteiraRepositoryMock
            .Setup(x => x.ObterPorIdAsync(carteiraId))
            .ReturnsAsync(carteira);

        // Act
        var resultado = await _carteiraService.ObterPorIdAsync(carteiraId, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Id.Should().Be(carteiraId);
        resultado.Data.Nome.Should().Be("Carteira Teste");
    }

    [Fact]
    public async Task ObterPorIdAsync_QuandoCarteiraNaoPertenceAoUsuario_DeveRetornarErro()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var carteiraId = 1L;

        _carteiraRepositoryMock
            .Setup(x => x.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId))
            .ReturnsAsync(false);

        // Act
        var resultado = await _carteiraService.ObterPorIdAsync(carteiraId, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.Errors.Should().Contain("Acesso negado: esta carteira não pertence ao usuário autenticado");
    }

    [Fact]
    public async Task CriarAsync_ComDadosValidos_DeveCriarCarteira()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var usuario = new Usuario
        {
            Id = usuarioId,
            Nome = "Usuario Teste",
            Email = "teste@teste.com",
            SenhaHash = "hash",
            CriadoEm = DateTimeOffset.UtcNow
        };

        var request = new CarteiraRequest
        {
            Nome = "Nova Carteira",
            Descricao = "Descrição"
        };

        var carteiraCriada = new Carteira
        {
            Id = 1,
            UsuarioId = usuarioId,
            Nome = request.Nome,
            Descricao = request.Descricao,
            CriadaEm = DateTimeOffset.UtcNow
        };

        _usuarioRepositoryMock
            .Setup(x => x.ObterPorIdAsync(usuarioId))
            .ReturnsAsync(usuario);

        _carteiraRepositoryMock
            .Setup(x => x.SalvarAsync(It.IsAny<Carteira>()))
            .ReturnsAsync(carteiraCriada);

        // Act
        var resultado = await _carteiraService.CriarAsync(request, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Nome.Should().Be(request.Nome);
        resultado.Data.Descricao.Should().Be(request.Descricao);
    }

    [Fact]
    public async Task CriarAsync_ComNomeVazio_DeveRetornarErroValidacao()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var request = new CarteiraRequest
        {
            Nome = "", // Nome vazio
            Descricao = "Descrição"
        };

        // Act
        var resultado = await _carteiraService.CriarAsync(request, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.ValidationErrors.Should().ContainKey("Nome");
    }

    [Fact]
    public async Task CriarAsync_ComNomeMuitoCurto_DeveRetornarErroValidacao()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var request = new CarteiraRequest
        {
            Nome = "AB", // Menos de 3 caracteres
            Descricao = "Descrição"
        };

        // Act
        var resultado = await _carteiraService.CriarAsync(request, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.ValidationErrors.Should().ContainKey("Nome");
    }

    [Fact]
    public async Task AtualizarAsync_ComDadosValidos_DeveAtualizarCarteira()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var carteiraId = 1L;

        var carteiraExistente = new Carteira
        {
            Id = carteiraId,
            UsuarioId = usuarioId,
            Nome = "Nome Antigo",
            Descricao = "Descrição Antiga",
            CriadaEm = DateTimeOffset.UtcNow
        };

        var request = new CarteiraRequest
        {
            Nome = "Nome Atualizado",
            Descricao = "Descrição Atualizada"
        };

        _carteiraRepositoryMock
            .Setup(x => x.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId))
            .ReturnsAsync(true);

        _carteiraRepositoryMock
            .Setup(x => x.ObterPorIdAsync(carteiraId))
            .ReturnsAsync(carteiraExistente);

        _carteiraRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Carteira>()))
            .ReturnsAsync((Carteira c) => c);

        // Act
        var resultado = await _carteiraService.AtualizarAsync(carteiraId, request, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Nome.Should().Be(request.Nome);
        resultado.Data.Descricao.Should().Be(request.Descricao);
    }

    [Fact]
    public async Task ExcluirAsync_CarteiraSemTransacoes_DeveExcluirComSucesso()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var carteiraId = 1L;

        var carteira = new Carteira
        {
            Id = carteiraId,
            UsuarioId = usuarioId,
            Nome = "Carteira Teste",
            CriadaEm = DateTimeOffset.UtcNow
        };

        _carteiraRepositoryMock
            .Setup(x => x.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId))
            .ReturnsAsync(true);

        _carteiraRepositoryMock
            .Setup(x => x.ObterPorIdAsync(carteiraId))
            .ReturnsAsync(carteira);

        _carteiraRepositoryMock
            .Setup(x => x.ExcluirAsync(carteiraId))
            .ReturnsAsync(true);

        // Act
        var resultado = await _carteiraService.ExcluirAsync(carteiraId, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ObterComPosicaoPorIdAsync_QuandoCarteiraExiste_DeveRetornarComPosicao()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var carteiraId = 1L;

        var carteira = new Carteira
        {
            Id = carteiraId,
            UsuarioId = usuarioId,
            Nome = "Carteira Teste",
            CriadaEm = DateTimeOffset.UtcNow
        };

        var posicaoConsolidada = new PosicaoConsolidadaResponse
        {
            CarteiraId = carteiraId,
            CarteiraNome = "Carteira Teste",
            ValorTotalInvestido = 10000m,
            LucroTotal = 1000m,
            RentabilidadeTotal = 10m
        };

        var transacoes = new List<Transacao>
        {
            new Transacao
            {
                Id = Guid.NewGuid(),
                CarteiraId = carteiraId,
                AtivoId = 1,
                Quantidade = 100,
                Preco = 10m,
                TipoTransacao = TipoTransacao.Compra,
                DataTransacao = DateTimeOffset.UtcNow
            }
        };

        _carteiraRepositoryMock
            .Setup(x => x.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId))
            .ReturnsAsync(true);

        _carteiraRepositoryMock
            .Setup(x => x.ObterPorIdAsync(carteiraId))
            .ReturnsAsync(carteira);

        _posicaoServiceMock
            .Setup(x => x.CalcularPosicaoAsync(carteiraId, usuarioId))
            .ReturnsAsync(Result<PosicaoConsolidadaResponse>.Success(posicaoConsolidada));

        _transacaoRepositoryMock
            .Setup(x => x.ObterPorCarteiraIdAsync(carteiraId))
            .ReturnsAsync(transacoes);

        // Act
        var resultado = await _carteiraService.ObterComPosicaoPorIdAsync(carteiraId, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Id.Should().Be(carteiraId);
        resultado.Data.ValorTotal.Should().Be(10000m);
        resultado.Data.RentabilidadeTotal.Should().NotBeNull();
    }
}
