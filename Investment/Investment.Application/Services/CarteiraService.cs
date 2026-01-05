using Gridify;
using Investment.Application.DTOs.Carteira;
using Investment.Application.DTOs.Posicao;
using Investment.Application.Mappers;
using Investment.Domain.Common;
using Investment.Infrastructure.Repositories;

namespace Investment.Application.Services;

public class CarteiraService(
    ICarteiraRepository carteiraRepository,
    IUsuarioRepository usuarioRepository,
    IPosicaoService posicaoService,
    ITransacaoRepository transacaoRepository) : ICarteiraService
{
    public async Task<Result<Paging<CarteiraResponse>>> ObterAsync(GridifyQuery query, Guid usuarioId)
    {
        var paging = await carteiraRepository.ObterPorUsuarioAsync(query, usuarioId);

        var responsePaging = new Paging<CarteiraResponse>
        {
            Count = paging.Count,
            Data = paging.Data.Select(CarteiraMapper.ToResponse).ToList()
        };

        return Result<Paging<CarteiraResponse>>.Success(responsePaging);
    }

    public async Task<Result<Paging<CarteiraComPosicaoResponse>>> ObterComPosicaoAsync(GridifyQuery query,
        Guid usuarioId)
    {
        var paging = await carteiraRepository.ObterPorUsuarioAsync(query, usuarioId);

        var carteirasComPosicao = new List<CarteiraComPosicaoResponse>();

        foreach (var carteira in paging.Data)
        {
            var posicaoResult = await posicaoService.CalcularPosicaoAsync(carteira.Id, usuarioId);
            var (lucroTotal, rentabilidadeTotal) = await CalcularRentabilidadeAsync(carteira.Id, posicaoResult);
            var carteiraComPosicao = new CarteiraComPosicaoResponse
            {
                Id = carteira.Id,
                UsuarioId = carteira.UsuarioId,
                Nome = carteira.Nome,
                Descricao = carteira.Descricao,
                CriadaEm = carteira.CriadaEm,
                TotalAtivos = carteira.CarteirasAtivos?.Count ?? 0,
                TotalTransacoes = carteira.Transacoes?.Count ?? 0,
                ValorTotal = posicaoResult.IsSuccess && posicaoResult.Data != null
                    ? posicaoResult.Data.ValorTotalInvestido
                    : 0,
                LucroTotal = lucroTotal,
                RentabilidadeTotal = rentabilidadeTotal
            };
            carteirasComPosicao.Add(carteiraComPosicao);
        }

        var responsePaging = new Paging<CarteiraComPosicaoResponse>
        {
            Count = paging.Count,
            Data = carteirasComPosicao
        };
        return Result<Paging<CarteiraComPosicaoResponse>>.Success(responsePaging);
    }

    public async Task<Result<List<CarteiraResponse>>> ObterPorUsuarioAsync(Guid usuarioId)
    {
        var carteiras = await carteiraRepository.ObterPorUsuarioIdAsync(usuarioId);
        var responses = CarteiraMapper.ToResponseList(carteiras);
        return Result<List<CarteiraResponse>>.Success(responses);
    }

    public async Task<Result<CarteiraResponse>> ObterPorIdAsync(long id, Guid usuarioId)
    {
        var pertenceAoUsuario = await carteiraRepository.UsuarioPossuiCarteiraAsync(usuarioId, id);
        if (!pertenceAoUsuario)
            return Result<CarteiraResponse>.Failure("Acesso negado: esta carteira não pertence ao usuário autenticado");

        var carteira = await carteiraRepository.ObterPorIdAsync(id);
        if (carteira == null) return Result<CarteiraResponse>.Failure($"Carteira com ID {id} não encontrada");

        var response = CarteiraMapper.ToResponse(carteira);
        return Result<CarteiraResponse>.Success(response);
    }

    public async Task<Result<CarteiraComPosicaoResponse>> ObterComPosicaoPorIdAsync(long id, Guid usuarioId)
    {
        var pertenceAoUsuario = await carteiraRepository.UsuarioPossuiCarteiraAsync(usuarioId, id);
        if (!pertenceAoUsuario)
            return Result<CarteiraComPosicaoResponse>.Failure(
                "Acesso negado: esta carteira não pertence ao usuário autenticado");

        var carteira = await carteiraRepository.ObterPorIdAsync(id);
        if (carteira == null) return Result<CarteiraComPosicaoResponse>.Failure($"Carteira com ID {id} não encontrada");

        var posicaoResult = await posicaoService.CalcularPosicaoAsync(carteira.Id, usuarioId);
        var (lucroTotal, rentabilidadeTotal) = await CalcularRentabilidadeAsync(carteira.Id, posicaoResult);

        var response = new CarteiraComPosicaoResponse
        {
            Id = carteira.Id,
            UsuarioId = carteira.UsuarioId,
            Nome = carteira.Nome,
            Descricao = carteira.Descricao,
            CriadaEm = carteira.CriadaEm,
            TotalAtivos = carteira.CarteirasAtivos?.Count ?? 0,
            TotalTransacoes = carteira.Transacoes?.Count ?? 0,
            ValorTotal = posicaoResult.IsSuccess && posicaoResult.Data != null
                ? posicaoResult.Data.ValorTotalInvestido
                : 0,
            LucroTotal = lucroTotal,
            RentabilidadeTotal = rentabilidadeTotal
        };

        return Result<CarteiraComPosicaoResponse>.Success(response);
    }

    public async Task<Result<CarteiraComDetalhesResponse>> ObterComDetalhesAsync(long id, Guid usuarioId)
    {
        var pertenceAoUsuario = await carteiraRepository.UsuarioPossuiCarteiraAsync(usuarioId, id);
        if (!pertenceAoUsuario)
            return Result<CarteiraComDetalhesResponse>.Failure(
                "Acesso negado: esta carteira não pertence ao usuário autenticado");

        var carteira = await carteiraRepository.ObterCompletoAsync(id);
        if (carteira == null)
            return Result<CarteiraComDetalhesResponse>.Failure($"Carteira com ID {id} não encontrada");

        var response = CarteiraMapper.ToResponseComDetalhes(carteira);
        return Result<CarteiraComDetalhesResponse>.Success(response);
    }

    public async Task<Result<CarteiraResponse>> CriarAsync(CarteiraRequest request, Guid usuarioId)
    {
        var validationErrors = ValidarRequest(request);
        if (validationErrors.Any()) return Result<CarteiraResponse>.Failure(validationErrors);
        var usuarioExiste = await usuarioRepository.ObterPorIdAsync(usuarioId);
        if (usuarioExiste == null) return Result<CarteiraResponse>.Failure("Usuário não encontrado");

        var carteira = CarteiraMapper.ToEntity(request, usuarioId);
        var carteiraSalva = await carteiraRepository.SalvarAsync(carteira);
        var response = CarteiraMapper.ToResponse(carteiraSalva);
        return Result<CarteiraResponse>.Success(response);
    }

    public async Task<Result<CarteiraResponse>> AtualizarAsync(long id, CarteiraRequest request, Guid usuarioId)
    {
        var pertenceAoUsuario = await carteiraRepository.UsuarioPossuiCarteiraAsync(usuarioId, id);
        if (!pertenceAoUsuario)
            return Result<CarteiraResponse>.Failure("Acesso negado: esta carteira não pertence ao usuário autenticado");
        var validationErrors = ValidarRequest(request);
        if (validationErrors.Any()) return Result<CarteiraResponse>.Failure(validationErrors);

        var carteiraExistente = await carteiraRepository.ObterPorIdAsync(id);
        if (carteiraExistente == null) return Result<CarteiraResponse>.Failure($"Carteira com ID {id} não encontrada");

        CarteiraMapper.UpdateEntity(carteiraExistente, request);
        var carteiraAtualizada = await carteiraRepository.AtualizarAsync(carteiraExistente);
        var response = CarteiraMapper.ToResponse(carteiraAtualizada);
        return Result<CarteiraResponse>.Success(response);
    }

    public async Task<Result> ExcluirAsync(long id, Guid usuarioId)
    {
        var pertenceAoUsuario = await carteiraRepository.UsuarioPossuiCarteiraAsync(usuarioId, id);
        if (!pertenceAoUsuario)
            return Result.Failure("Acesso negado: esta carteira não pertence ao usuário autenticado");

        var carteiraExistente = await carteiraRepository.ObterPorIdAsync(id);
        if (carteiraExistente == null) return Result.Failure($"Carteira com ID {id} não encontrada");

        try
        {
            var excluido = await carteiraRepository.ExcluirAsync(id);
            if (!excluido) return Result.Failure("Erro ao excluir a carteira");

            return Result.Success();
        }
        catch
        {
            return Result.Failure("Não é possível excluir a carteira pois ela possui transações associadas");
        }
    }

    private async Task<(decimal? lucroTotal, decimal? rentabilidadeTotal)> CalcularRentabilidadeAsync(
        long carteiraId,
        Result<PosicaoConsolidadaResponse> posicaoResult)
    {
        if (!posicaoResult.IsSuccess || posicaoResult.Data == null) return (null, null);
        var transacoes = await transacaoRepository.ObterPorCarteiraIdAsync(carteiraId);
        if (!transacoes.Any()) return (0, 0);
        decimal totalInvestido = 0;
        decimal totalRecebidoVendas = 0;
        decimal totalProventos = 0;
        foreach (var transacao in transacoes)
            switch (transacao.TipoTransacao)
            {
                case TipoTransacao.Compra:
                    totalInvestido += transacao.Quantidade * transacao.Preco;
                    break;
                case TipoTransacao.Venda:
                    totalRecebidoVendas += Math.Abs(transacao.Quantidade) * transacao.Preco;
                    break;
                case TipoTransacao.Dividendo:
                case TipoTransacao.JCP:
                    totalProventos += transacao.Preco * Math.Abs(transacao.Quantidade);
                    break;
            }

        var valorAtualCarteira = posicaoResult.Data.ValorTotalInvestido;
        var lucroTotal = valorAtualCarteira + totalRecebidoVendas + totalProventos - totalInvestido;
        decimal? rentabilidadeTotal = totalInvestido > 0
            ? lucroTotal / totalInvestido * 100
            : null;
        return (lucroTotal, rentabilidadeTotal);
    }

    private Dictionary<string, List<string>> ValidarRequest(CarteiraRequest request)
    {
        var errors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(request.Nome))
            errors.Add("Nome", new List<string> { "Nome é obrigatório" });
        else if (request.Nome.Length < 3 || request.Nome.Length > 100)
            errors.Add("Nome", new List<string> { "Nome deve ter entre 3 e 100 caracteres" });

        if (!string.IsNullOrWhiteSpace(request.Descricao) && request.Descricao.Length > 500)
            errors.Add("Descricao", new List<string> { "Descrição deve ter no máximo 500 caracteres" });

        return errors;
    }
}