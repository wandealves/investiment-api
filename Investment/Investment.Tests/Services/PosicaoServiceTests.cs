using FluentAssertions;
using Investment.Application.Services;
using Investment.Domain.Common;
using Investment.Domain.Entidades;
using Investment.Infrastructure.Repositories;
using Moq;

namespace Investment.Tests.Services;

public class PosicaoServiceTests
{
    private readonly Mock<ITransacaoRepository> _transacaoRepositoryMock;
    private readonly Mock<ICarteiraRepository> _carteiraRepositoryMock;
    private readonly Mock<IAtivoRepository> _ativoRepositoryMock;
    private readonly PosicaoService _posicaoService;

    public PosicaoServiceTests()
    {
        _transacaoRepositoryMock = new Mock<ITransacaoRepository>();
        _carteiraRepositoryMock = new Mock<ICarteiraRepository>();
        _ativoRepositoryMock = new Mock<IAtivoRepository>();

        _posicaoService = new PosicaoService(
            _transacaoRepositoryMock.Object,
            _carteiraRepositoryMock.Object,
            _ativoRepositoryMock.Object
        );
    }

    [Fact]
    public async Task CalcularPosicaoAsync_CarteiraNaoPertenceAoUsuario_DeveRetornarErro()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var carteiraId = 1L;

        _carteiraRepositoryMock
            .Setup(x => x.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId))
            .ReturnsAsync(false);

        // Act
        var resultado = await _posicaoService.CalcularPosicaoAsync(carteiraId, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.Errors.Should().Contain("Acesso negado: esta carteira não pertence ao usuário autenticado");
    }

    [Fact]
    public async Task CalcularPosicaoAsync_CarteiraNaoEncontrada_DeveRetornarErro()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var carteiraId = 1L;

        _carteiraRepositoryMock
            .Setup(x => x.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId))
            .ReturnsAsync(true);

        _carteiraRepositoryMock
            .Setup(x => x.ObterPorIdAsync(carteiraId))
            .ReturnsAsync((Carteira?)null);

        // Act
        var resultado = await _posicaoService.CalcularPosicaoAsync(carteiraId, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.Errors.Should().Contain($"Carteira com ID {carteiraId} não encontrada");
    }

    [Fact]
    public async Task CalcularPosicaoAsync_ApenasCompras_DeveCalcularPrecoMedioPonderado()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var carteiraId = 1L;
        var ativoId = 1L;

        var carteira = new Carteira
        {
            Id = carteiraId,
            UsuarioId = usuarioId,
            Nome = "Carteira Teste",
            CriadaEm = DateTimeOffset.UtcNow
        };

        var ativo = new Ativo
        {
            Id = ativoId,
            Codigo = "PETR4",
            Nome = "Petrobras PN",
            Tipo = TipoAtivo.Acao
        };

        var transacoes = new List<Transacao>
        {
            new Transacao
            {
                Id = Guid.NewGuid(),
                CarteiraId = carteiraId,
                AtivoId = ativoId,
                Ativo = ativo,
                Quantidade = 100,
                Preco = 10m,
                TipoTransacao = TipoTransacao.Compra,
                DataTransacao = DateTimeOffset.UtcNow.AddDays(-2)
            },
            new Transacao
            {
                Id = Guid.NewGuid(),
                CarteiraId = carteiraId,
                AtivoId = ativoId,
                Ativo = ativo,
                Quantidade = 50,
                Preco = 12m,
                TipoTransacao = TipoTransacao.Compra,
                DataTransacao = DateTimeOffset.UtcNow.AddDays(-1)
            }
        };

        _carteiraRepositoryMock
            .Setup(x => x.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId))
            .ReturnsAsync(true);

        _carteiraRepositoryMock
            .Setup(x => x.ObterPorIdAsync(carteiraId))
            .ReturnsAsync(carteira);

        _transacaoRepositoryMock
            .Setup(x => x.ObterPorCarteiraIdAsync(carteiraId))
            .ReturnsAsync(transacoes);

        // Act
        var resultado = await _posicaoService.CalcularPosicaoAsync(carteiraId, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Posicoes.Should().HaveCount(1);

        var posicao = resultado.Data.Posicoes.First();
        posicao.QuantidadeAtual.Should().Be(150); // 100 + 50

        // Preço médio ponderado: (100*10 + 50*12) / 150 = 1600 / 150 = 10.67
        posicao.PrecoMedio.Should().BeApproximately(10.67m, 0.01m);
        posicao.ValorInvestido.Should().BeApproximately(1600m, 0.01m); // 150 * 10.67
    }

    [Fact]
    public async Task CalcularPosicaoAsync_CompraEVenda_DeveCalcularCorretamente()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var carteiraId = 1L;
        var ativoId = 1L;

        var carteira = new Carteira
        {
            Id = carteiraId,
            UsuarioId = usuarioId,
            Nome = "Carteira Teste",
            CriadaEm = DateTimeOffset.UtcNow
        };

        var ativo = new Ativo
        {
            Id = ativoId,
            Codigo = "VALE3",
            Nome = "Vale ON",
            Tipo = TipoAtivo.Acao
        };

        var transacoes = new List<Transacao>
        {
            new Transacao
            {
                Id = Guid.NewGuid(),
                CarteiraId = carteiraId,
                AtivoId = ativoId,
                Ativo = ativo,
                Quantidade = 200,
                Preco = 50m,
                TipoTransacao = TipoTransacao.Compra,
                DataTransacao = DateTimeOffset.UtcNow.AddDays(-5)
            },
            new Transacao
            {
                Id = Guid.NewGuid(),
                CarteiraId = carteiraId,
                AtivoId = ativoId,
                Ativo = ativo,
                Quantidade = 100,
                Preco = 55m,
                TipoTransacao = TipoTransacao.Venda,
                DataTransacao = DateTimeOffset.UtcNow.AddDays(-2)
            }
        };

        _carteiraRepositoryMock
            .Setup(x => x.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId))
            .ReturnsAsync(true);

        _carteiraRepositoryMock
            .Setup(x => x.ObterPorIdAsync(carteiraId))
            .ReturnsAsync(carteira);

        _transacaoRepositoryMock
            .Setup(x => x.ObterPorCarteiraIdAsync(carteiraId))
            .ReturnsAsync(transacoes);

        // Act
        var resultado = await _posicaoService.CalcularPosicaoAsync(carteiraId, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Posicoes.Should().HaveCount(1);

        var posicao = resultado.Data.Posicoes.First();
        posicao.QuantidadeAtual.Should().Be(100); // 200 - 100
        posicao.PrecoMedio.Should().Be(50m); // Preço médio mantém
        posicao.ValorInvestido.Should().Be(5000m); // 100 * 50
    }

    [Fact]
    public async Task CalcularPosicaoAsync_ComDividendos_DeveAcumularProventos()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var carteiraId = 1L;
        var ativoId = 1L;

        var carteira = new Carteira
        {
            Id = carteiraId,
            UsuarioId = usuarioId,
            Nome = "Carteira Teste",
            CriadaEm = DateTimeOffset.UtcNow
        };

        var ativo = new Ativo
        {
            Id = ativoId,
            Codigo = "ITSA4",
            Nome = "Itaúsa PN",
            Tipo = TipoAtivo.Acao
        };

        var transacoes = new List<Transacao>
        {
            new Transacao
            {
                Id = Guid.NewGuid(),
                CarteiraId = carteiraId,
                AtivoId = ativoId,
                Ativo = ativo,
                Quantidade = 1000,
                Preco = 10m,
                TipoTransacao = TipoTransacao.Compra,
                DataTransacao = DateTimeOffset.UtcNow.AddDays(-30)
            },
            new Transacao
            {
                Id = Guid.NewGuid(),
                CarteiraId = carteiraId,
                AtivoId = ativoId,
                Ativo = ativo,
                Quantidade = 1000, // Quantidade de ações que receberam dividendos
                Preco = 0.50m, // Valor do dividendo por ação
                TipoTransacao = TipoTransacao.Dividendo,
                DataTransacao = DateTimeOffset.UtcNow.AddDays(-10)
            }
        };

        _carteiraRepositoryMock
            .Setup(x => x.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId))
            .ReturnsAsync(true);

        _carteiraRepositoryMock
            .Setup(x => x.ObterPorIdAsync(carteiraId))
            .ReturnsAsync(carteira);

        _transacaoRepositoryMock
            .Setup(x => x.ObterPorCarteiraIdAsync(carteiraId))
            .ReturnsAsync(transacoes);

        // Act
        var resultado = await _posicaoService.CalcularPosicaoAsync(carteiraId, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Posicoes.Should().HaveCount(1);

        var posicao = resultado.Data.Posicoes.First();
        posicao.QuantidadeAtual.Should().Be(1000);
        posicao.PrecoMedio.Should().Be(10m);
        posicao.DividendosRecebidos.Should().Be(500m); // 1000 * 0.50
    }

    [Fact]
    public async Task CalcularPosicaoAsync_ComSplit_DeveAjustarQuantidadeEPreco()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var carteiraId = 1L;
        var ativoId = 1L;

        var carteira = new Carteira
        {
            Id = carteiraId,
            UsuarioId = usuarioId,
            Nome = "Carteira Teste",
            CriadaEm = DateTimeOffset.UtcNow
        };

        var ativo = new Ativo
        {
            Id = ativoId,
            Codigo = "MGLU3",
            Nome = "Magazine Luiza ON",
            Tipo = TipoAtivo.Acao
        };

        var transacoes = new List<Transacao>
        {
            new Transacao
            {
                Id = Guid.NewGuid(),
                CarteiraId = carteiraId,
                AtivoId = ativoId,
                Ativo = ativo,
                Quantidade = 100,
                Preco = 20m,
                TipoTransacao = TipoTransacao.Compra,
                DataTransacao = DateTimeOffset.UtcNow.AddDays(-30)
            },
            new Transacao
            {
                Id = Guid.NewGuid(),
                CarteiraId = carteiraId,
                AtivoId = ativoId,
                Ativo = ativo,
                Quantidade = 2, // Split 2:1
                Preco = 0,
                TipoTransacao = TipoTransacao.Split,
                DataTransacao = DateTimeOffset.UtcNow.AddDays(-10)
            }
        };

        _carteiraRepositoryMock
            .Setup(x => x.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId))
            .ReturnsAsync(true);

        _carteiraRepositoryMock
            .Setup(x => x.ObterPorIdAsync(carteiraId))
            .ReturnsAsync(carteira);

        _transacaoRepositoryMock
            .Setup(x => x.ObterPorCarteiraIdAsync(carteiraId))
            .ReturnsAsync(transacoes);

        // Act
        var resultado = await _posicaoService.CalcularPosicaoAsync(carteiraId, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Posicoes.Should().HaveCount(1);

        var posicao = resultado.Data.Posicoes.First();
        posicao.QuantidadeAtual.Should().Be(200); // 100 * 2
        posicao.PrecoMedio.Should().Be(10m); // 20 / 2
        posicao.ValorInvestido.Should().Be(2000m); // Valor investido mantém
    }

    [Fact]
    public async Task CalcularPosicaoAsync_PosicaoZerada_NaoDeveIncluirNaLista()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var carteiraId = 1L;
        var ativoId = 1L;

        var carteira = new Carteira
        {
            Id = carteiraId,
            UsuarioId = usuarioId,
            Nome = "Carteira Teste",
            CriadaEm = DateTimeOffset.UtcNow
        };

        var ativo = new Ativo
        {
            Id = ativoId,
            Codigo = "PETR4",
            Nome = "Petrobras PN",
            Tipo = TipoAtivo.Acao
        };

        var transacoes = new List<Transacao>
        {
            new Transacao
            {
                Id = Guid.NewGuid(),
                CarteiraId = carteiraId,
                AtivoId = ativoId,
                Ativo = ativo,
                Quantidade = 100,
                Preco = 30m,
                TipoTransacao = TipoTransacao.Compra,
                DataTransacao = DateTimeOffset.UtcNow.AddDays(-10)
            },
            new Transacao
            {
                Id = Guid.NewGuid(),
                CarteiraId = carteiraId,
                AtivoId = ativoId,
                Ativo = ativo,
                Quantidade = 100,
                Preco = 35m,
                TipoTransacao = TipoTransacao.Venda,
                DataTransacao = DateTimeOffset.UtcNow.AddDays(-5)
            }
        };

        _carteiraRepositoryMock
            .Setup(x => x.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId))
            .ReturnsAsync(true);

        _carteiraRepositoryMock
            .Setup(x => x.ObterPorIdAsync(carteiraId))
            .ReturnsAsync(carteira);

        _transacaoRepositoryMock
            .Setup(x => x.ObterPorCarteiraIdAsync(carteiraId))
            .ReturnsAsync(transacoes);

        // Act
        var resultado = await _posicaoService.CalcularPosicaoAsync(carteiraId, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Posicoes.Should().BeEmpty(); // Posição zerada não deve aparecer
    }
}
