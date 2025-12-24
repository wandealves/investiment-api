using FluentAssertions;
using Investment.Application.DTOs.Transacao;
using Investment.Application.Services;
using Investment.Domain.Common;
using Investment.Domain.Entidades;
using Investment.Infrastructure.Repositories;
using Moq;

namespace Investment.Tests.Services;

public class TransacaoServiceTests
{
    private readonly Mock<ITransacaoRepository> _transacaoRepositoryMock;
    private readonly Mock<ICarteiraRepository> _carteiraRepositoryMock;
    private readonly Mock<IAtivoRepository> _ativoRepositoryMock;
    private readonly TransacaoService _transacaoService;

    public TransacaoServiceTests()
    {
        _transacaoRepositoryMock = new Mock<ITransacaoRepository>();
        _carteiraRepositoryMock = new Mock<ICarteiraRepository>();
        _ativoRepositoryMock = new Mock<IAtivoRepository>();

        _transacaoService = new TransacaoService(
            _transacaoRepositoryMock.Object,
            _carteiraRepositoryMock.Object,
            _ativoRepositoryMock.Object
        );
    }

    [Fact]
    public async Task CriarAsync_CompraComDadosValidos_DeveCriarTransacao()
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

        var request = new TransacaoRequest
        {
            CarteiraId = carteiraId,
            AtivoId = ativoId,
            Quantidade = 100,
            Preco = 30.50m,
            TipoTransacao = TipoTransacao.Compra,
            DataTransacao = DateTimeOffset.UtcNow
        };

        _carteiraRepositoryMock
            .Setup(x => x.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId))
            .ReturnsAsync(true);

        _carteiraRepositoryMock
            .Setup(x => x.ObterPorIdAsync(carteiraId))
            .ReturnsAsync(carteira);

        _ativoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ativoId))
            .ReturnsAsync(ativo);

        var transacaoSalva = new Transacao
        {
            Id = Guid.NewGuid(),
            CarteiraId = carteiraId,
            AtivoId = ativoId,
            Ativo = ativo,
            Quantidade = 100,
            Preco = 30.50m,
            TipoTransacao = TipoTransacao.Compra,
            DataTransacao = DateTimeOffset.UtcNow
        };

        _transacaoRepositoryMock
            .Setup(x => x.SalvarAsync(It.IsAny<Transacao>()))
            .ReturnsAsync(transacaoSalva);

        _transacaoRepositoryMock
            .Setup(x => x.ObterComDetalhesAsync(It.IsAny<Guid>()))
            .ReturnsAsync(transacaoSalva);

        // Act
        var resultado = await _transacaoService.CriarAsync(request, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Quantidade.Should().Be(100);
        resultado.Data.Preco.Should().Be(30.50m);
    }

    [Fact]
    public async Task CriarAsync_CarteiraNaoPertenceAoUsuario_DeveRetornarErro()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var carteiraId = 1L;

        var request = new TransacaoRequest
        {
            CarteiraId = carteiraId,
            AtivoId = 1,
            Quantidade = 100,
            Preco = 30m,
            TipoTransacao = TipoTransacao.Compra,
            DataTransacao = DateTimeOffset.UtcNow
        };

        _carteiraRepositoryMock
            .Setup(x => x.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId))
            .ReturnsAsync(false);

        var ativo = new Ativo
        {
            Id = 1,
            Codigo = "PETR4",
            Nome = "Petrobras PN",
            Tipo = TipoAtivo.Acao
        };

        _ativoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(1L))
            .ReturnsAsync(ativo);

        // Act
        var resultado = await _transacaoService.CriarAsync(request, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.ValidationErrors.Should().ContainKey("CarteiraId");
        resultado.ValidationErrors["CarteiraId"].Should().Contain("Carteira não encontrada ou não pertence ao usuário");
    }

    [Fact]
    public async Task CriarAsync_PrecoNegativo_DeveRetornarErroValidacao()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var request = new TransacaoRequest
        {
            CarteiraId = 1,
            AtivoId = 1,
            Quantidade = 100,
            Preco = -10m, // Preço negativo
            TipoTransacao = TipoTransacao.Compra,
            DataTransacao = DateTimeOffset.UtcNow
        };

        // Act
        var resultado = await _transacaoService.CriarAsync(request, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.ValidationErrors.Should().ContainKey("Preco");
    }

    [Fact]
    public async Task CriarAsync_QuantidadeZero_DeveRetornarErroValidacao()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var request = new TransacaoRequest
        {
            CarteiraId = 1,
            AtivoId = 1,
            Quantidade = 0, // Quantidade zero
            Preco = 30m,
            TipoTransacao = TipoTransacao.Compra,
            DataTransacao = DateTimeOffset.UtcNow
        };

        // Act
        var resultado = await _transacaoService.CriarAsync(request, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.ValidationErrors.Should().ContainKey("Quantidade");
    }

    [Fact]
    public async Task CriarAsync_DataFutura_DeveRetornarErroValidacao()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var request = new TransacaoRequest
        {
            CarteiraId = 1,
            AtivoId = 1,
            Quantidade = 100,
            Preco = 30m,
            TipoTransacao = TipoTransacao.Compra,
            DataTransacao = DateTimeOffset.UtcNow.AddDays(1) // Data futura
        };

        // Act
        var resultado = await _transacaoService.CriarAsync(request, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.ValidationErrors.Should().ContainKey("DataTransacao");
    }

    [Fact]
    public async Task CriarAsync_VendaSemSaldo_DeveRetornarErro()
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

        var request = new TransacaoRequest
        {
            CarteiraId = carteiraId,
            AtivoId = ativoId,
            Quantidade = 100, // Tentando vender 100
            Preco = 60m,
            TipoTransacao = TipoTransacao.Venda,
            DataTransacao = DateTimeOffset.UtcNow
        };

        var transacoesExistentes = new List<Transacao>
        {
            new Transacao
            {
                Id = Guid.NewGuid(),
                CarteiraId = carteiraId,
                AtivoId = ativoId,
                Ativo = ativo,
                Quantidade = 50, // Só tem 50 em carteira
                Preco = 50m,
                TipoTransacao = TipoTransacao.Compra,
                DataTransacao = DateTimeOffset.UtcNow.AddDays(-10)
            }
        };

        _carteiraRepositoryMock
            .Setup(x => x.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId))
            .ReturnsAsync(true);

        _carteiraRepositoryMock
            .Setup(x => x.ObterPorIdAsync(carteiraId))
            .ReturnsAsync(carteira);

        _ativoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ativoId))
            .ReturnsAsync(ativo);

        _transacaoRepositoryMock
            .Setup(x => x.ObterPorCarteiraEAtivoAsync(carteiraId, ativoId))
            .ReturnsAsync(transacoesExistentes);

        _transacaoRepositoryMock
            .Setup(x => x.ObterPorCarteiraEAtivoAsync(carteiraId, ativoId))
            .ReturnsAsync(transacoesExistentes);

        // Act
        var resultado = await _transacaoService.CriarAsync(request, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.ValidationErrors.Should().ContainKey("Quantidade");
        resultado.ValidationErrors["Quantidade"].Should().Contain("Saldo insuficiente para realizar a venda");
    }

    [Fact]
    public async Task AtualizarAsync_ComDadosValidos_DeveAtualizarTransacao()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var transacaoId = Guid.NewGuid();
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

        var transacaoExistente = new Transacao
        {
            Id = transacaoId,
            CarteiraId = carteiraId,
            AtivoId = ativoId,
            Quantidade = 100,
            Preco = 10m,
            TipoTransacao = TipoTransacao.Compra,
            DataTransacao = DateTimeOffset.UtcNow.AddDays(-5)
        };

        var request = new TransacaoRequest
        {
            CarteiraId = carteiraId,
            AtivoId = ativoId,
            Quantidade = 150, // Alterando quantidade
            Preco = 11m, // Alterando preço
            TipoTransacao = TipoTransacao.Compra,
            DataTransacao = DateTimeOffset.UtcNow.AddDays(-5)
        };

        _transacaoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(transacaoId))
            .ReturnsAsync(transacaoExistente);

        _carteiraRepositoryMock
            .Setup(x => x.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId))
            .ReturnsAsync(true);

        _carteiraRepositoryMock
            .Setup(x => x.ObterPorIdAsync(carteiraId))
            .ReturnsAsync(carteira);

        _ativoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(ativoId))
            .ReturnsAsync(ativo);

        var transacaoAtualizada = new Transacao
        {
            Id = transacaoId,
            CarteiraId = carteiraId,
            AtivoId = ativoId,
            Ativo = ativo,
            Quantidade = 150,
            Preco = 11m,
            TipoTransacao = TipoTransacao.Compra,
            DataTransacao = DateTimeOffset.UtcNow.AddDays(-5)
        };

        _transacaoRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Transacao>()))
            .ReturnsAsync(transacaoAtualizada);

        _transacaoRepositoryMock
            .Setup(x => x.ObterComDetalhesAsync(transacaoId))
            .ReturnsAsync(transacaoAtualizada);

        // Act
        var resultado = await _transacaoService.AtualizarAsync(transacaoId, request, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Quantidade.Should().Be(150);
        resultado.Data.Preco.Should().Be(11m);
    }

    [Fact]
    public async Task ExcluirAsync_TransacaoExiste_DeveExcluirComSucesso()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var transacaoId = Guid.NewGuid();
        var carteiraId = 1L;

        var transacao = new Transacao
        {
            Id = transacaoId,
            CarteiraId = carteiraId,
            AtivoId = 1,
            Quantidade = 100,
            Preco = 30m,
            TipoTransacao = TipoTransacao.Compra,
            DataTransacao = DateTimeOffset.UtcNow
        };

        _transacaoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(transacaoId))
            .ReturnsAsync(transacao);

        _carteiraRepositoryMock
            .Setup(x => x.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId))
            .ReturnsAsync(true);

        _transacaoRepositoryMock
            .Setup(x => x.ExcluirAsync(transacaoId))
            .ReturnsAsync(true);

        // Act
        var resultado = await _transacaoService.ExcluirAsync(transacaoId, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ObterPorIdAsync_TransacaoExiste_DeveRetornarTransacao()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var transacaoId = Guid.NewGuid();
        var carteiraId = 1L;

        var transacao = new Transacao
        {
            Id = transacaoId,
            CarteiraId = carteiraId,
            AtivoId = 1,
            Quantidade = 100,
            Preco = 30m,
            TipoTransacao = TipoTransacao.Compra,
            DataTransacao = DateTimeOffset.UtcNow
        };

        _transacaoRepositoryMock
            .Setup(x => x.ObterComDetalhesAsync(transacaoId))
            .ReturnsAsync(transacao);

        _carteiraRepositoryMock
            .Setup(x => x.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId))
            .ReturnsAsync(true);

        // Act
        var resultado = await _transacaoService.ObterPorIdAsync(transacaoId, usuarioId);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Id.Should().Be(transacaoId);
    }
}
