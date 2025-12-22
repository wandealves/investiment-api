using Gridify;
using Investment.Application.DTOs.Carteira;
using Investment.Application.DTOs.Posicao;
using Investment.Application.Mappers;
using Investment.Domain.Common;
using Investment.Infrastructure.Repositories;

namespace Investment.Application.Services;

public class CarteiraService : ICarteiraService
{
    private readonly ICarteiraRepository _carteiraRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPosicaoService _posicaoService;
    private readonly ITransacaoRepository _transacaoRepository;

    public CarteiraService(
        ICarteiraRepository carteiraRepository,
        IUsuarioRepository usuarioRepository,
        IPosicaoService posicaoService,
        ITransacaoRepository transacaoRepository)
    {
        _carteiraRepository = carteiraRepository;
        _usuarioRepository = usuarioRepository;
        _posicaoService = posicaoService;
        _transacaoRepository = transacaoRepository;
    }

    public async Task<Result<Paging<CarteiraResponse>>> ObterAsync(GridifyQuery query, Guid usuarioId)
    {
        var paging = await _carteiraRepository.ObterPorUsuarioAsync(query, usuarioId);

        var responsePaging = new Paging<CarteiraResponse>
        {
            Count = paging.Count,
            Data = paging.Data.Select(CarteiraMapper.ToResponse).ToList()
        };

        return Result<Paging<CarteiraResponse>>.Success(responsePaging);
    }

    public async Task<Result<Paging<CarteiraComPosicaoResponse>>> ObterComPosicaoAsync(GridifyQuery query, Guid usuarioId)
    {
        var paging = await _carteiraRepository.ObterPorUsuarioAsync(query, usuarioId);

        var carteirasComPosicao = new List<CarteiraComPosicaoResponse>();

        foreach (var carteira in paging.Data)
        {
            // Calcular posição da carteira
            var posicaoResult = await _posicaoService.CalcularPosicaoAsync(carteira.Id, usuarioId);

            // Calcular rentabilidade baseada nas transações
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

    private async Task<(decimal? lucroTotal, decimal? rentabilidadeTotal)> CalcularRentabilidadeAsync(
        long carteiraId,
        Result<PosicaoConsolidadaResponse> posicaoResult)
    {
        // Se não conseguiu calcular posição, retorna null
        if (!posicaoResult.IsSuccess || posicaoResult.Data == null)
        {
            return (null, null);
        }

        // Buscar todas as transações da carteira
        var transacoes = await _transacaoRepository.ObterPorCarteiraIdAsync(carteiraId);

        if (!transacoes.Any())
        {
            return (0, 0);
        }

        decimal totalInvestido = 0;     // Total gasto em compras
        decimal totalRecebidoVendas = 0; // Total recebido em vendas
        decimal totalProventos = 0;      // Total recebido em dividendos e JCP

        foreach (var transacao in transacoes)
        {
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

                // Bonus, Split e Grupamento não afetam o cálculo de rentabilidade monetária
            }
        }

        // Valor atual da carteira (posições em aberto)
        var valorAtualCarteira = posicaoResult.Data.ValorTotalInvestido;

        // Cálculo do lucro total:
        // Lucro = (Valor Atual + Total Vendido + Proventos) - Total Investido
        var lucroTotal = (valorAtualCarteira + totalRecebidoVendas + totalProventos) - totalInvestido;

        // Cálculo da rentabilidade percentual:
        // Rentabilidade = (Lucro / Total Investido) * 100
        decimal? rentabilidadeTotal = totalInvestido > 0
            ? (lucroTotal / totalInvestido) * 100
            : null;

        return (lucroTotal, rentabilidadeTotal);
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
