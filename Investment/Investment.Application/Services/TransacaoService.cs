using Investment.Application.DTOs.Transacao;
using Investment.Application.Mappers;
using Investment.Domain.Common;
using Investment.Infrastructure.Repositories;

namespace Investment.Application.Services;

public class TransacaoService : ITransacaoService
{
    private readonly ITransacaoRepository _transacaoRepository;
    private readonly ICarteiraRepository _carteiraRepository;
    private readonly IAtivoRepository _ativoRepository;

    public TransacaoService(
        ITransacaoRepository transacaoRepository,
        ICarteiraRepository carteiraRepository,
        IAtivoRepository ativoRepository)
    {
        _transacaoRepository = transacaoRepository;
        _carteiraRepository = carteiraRepository;
        _ativoRepository = ativoRepository;
    }

    public async Task<Result<TransacaoResponse>> ObterPorIdAsync(Guid id, Guid usuarioId)
    {
        var transacao = await _transacaoRepository.ObterComDetalhesAsync(id);

        if (transacao == null)
        {
            return Result<TransacaoResponse>.Failure($"Transação com ID {id} não encontrada");
        }

        // Verificar se a carteira pertence ao usuário
        var pertenceAoUsuario = await _carteiraRepository.UsuarioPossuiCarteiraAsync(usuarioId, transacao.CarteiraId);
        if (!pertenceAoUsuario)
        {
            return Result<TransacaoResponse>.Failure("Acesso negado: você não tem permissão para acessar esta transação");
        }

        var response = TransacaoMapper.ToResponse(transacao);
        return Result<TransacaoResponse>.Success(response);
    }

    public async Task<Result<List<TransacaoResponse>>> ObterPorCarteiraAsync(long carteiraId, Guid usuarioId)
    {
        // Verificar ownership
        var pertenceAoUsuario = await _carteiraRepository.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId);
        if (!pertenceAoUsuario)
        {
            return Result<List<TransacaoResponse>>.Failure("Acesso negado: esta carteira não pertence ao usuário autenticado");
        }

        var transacoes = await _transacaoRepository.ObterPorCarteiraIdAsync(carteiraId);
        var responses = TransacaoMapper.ToResponseList(transacoes);
        return Result<List<TransacaoResponse>>.Success(responses);
    }

    public async Task<Result<List<TransacaoResponse>>> ObterPorPeriodoAsync(long carteiraId, DateTime inicio, DateTime fim, Guid usuarioId)
    {
        // Verificar ownership
        var pertenceAoUsuario = await _carteiraRepository.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId);
        if (!pertenceAoUsuario)
        {
            return Result<List<TransacaoResponse>>.Failure("Acesso negado: esta carteira não pertence ao usuário autenticado");
        }

        var transacoes = await _transacaoRepository.ObterPorCarteiraEPeriodoAsync(carteiraId, inicio, fim);
        var responses = TransacaoMapper.ToResponseList(transacoes);
        return Result<List<TransacaoResponse>>.Success(responses);
    }

    public async Task<Result<TransacaoResponse>> CriarAsync(TransacaoRequest request, Guid usuarioId)
    {
        // Validar request
        var validationErrors = await ValidarRequestAsync(request, usuarioId);
        if (validationErrors.Any())
        {
            return Result<TransacaoResponse>.Failure(validationErrors);
        }

        // Para vendas, verificar saldo suficiente
        if (request.TipoTransacao == TipoTransacao.Venda)
        {
            var saldoSuficiente = await VerificarSaldoSuficienteAsync(request.CarteiraId, request.AtivoId, request.Quantidade);
            if (!saldoSuficiente)
            {
                var errors = new Dictionary<string, List<string>>
                {
                    { "Quantidade", new List<string> { "Saldo insuficiente para realizar a venda" } }
                };
                return Result<TransacaoResponse>.Failure(errors);
            }
        }

        var transacao = TransacaoMapper.ToEntity(request);
        var transacaoSalva = await _transacaoRepository.SalvarAsync(transacao);

        // Buscar transação completa com relacionamentos
        var transacaoCompleta = await _transacaoRepository.ObterComDetalhesAsync(transacaoSalva.Id);
        var response = TransacaoMapper.ToResponse(transacaoCompleta!);
        return Result<TransacaoResponse>.Success(response);
    }

    public async Task<Result<TransacaoResponse>> AtualizarAsync(Guid id, TransacaoRequest request, Guid usuarioId)
    {
        var transacaoExistente = await _transacaoRepository.ObterPorIdAsync(id);
        if (transacaoExistente == null)
        {
            return Result<TransacaoResponse>.Failure($"Transação com ID {id} não encontrada");
        }

        // Verificar ownership
        var pertenceAoUsuario = await _carteiraRepository.UsuarioPossuiCarteiraAsync(usuarioId, transacaoExistente.CarteiraId);
        if (!pertenceAoUsuario)
        {
            return Result<TransacaoResponse>.Failure("Acesso negado: você não tem permissão para atualizar esta transação");
        }

        // Validar request
        var validationErrors = await ValidarRequestAsync(request, usuarioId);
        if (validationErrors.Any())
        {
            return Result<TransacaoResponse>.Failure(validationErrors);
        }

        TransacaoMapper.UpdateEntity(transacaoExistente, request);
        var transacaoAtualizada = await _transacaoRepository.AtualizarAsync(transacaoExistente);

        // Buscar transação completa
        var transacaoCompleta = await _transacaoRepository.ObterComDetalhesAsync(transacaoAtualizada.Id);
        var response = TransacaoMapper.ToResponse(transacaoCompleta!);
        return Result<TransacaoResponse>.Success(response);
    }

    public async Task<Result> ExcluirAsync(Guid id, Guid usuarioId)
    {
        var transacaoExistente = await _transacaoRepository.ObterPorIdAsync(id);
        if (transacaoExistente == null)
        {
            return Result.Failure($"Transação com ID {id} não encontrada");
        }

        // Verificar ownership
        var pertenceAoUsuario = await _carteiraRepository.UsuarioPossuiCarteiraAsync(usuarioId, transacaoExistente.CarteiraId);
        if (!pertenceAoUsuario)
        {
            return Result.Failure("Acesso negado: você não tem permissão para excluir esta transação");
        }

        var excluido = await _transacaoRepository.ExcluirAsync(id);
        if (!excluido)
        {
            return Result.Failure("Erro ao excluir a transação");
        }

        return Result.Success();
    }

    private async Task<Dictionary<string, List<string>>> ValidarRequestAsync(TransacaoRequest request, Guid usuarioId)
    {
        var errors = new Dictionary<string, List<string>>();

        // Validar carteira
        if (request.CarteiraId <= 0)
        {
            errors.Add("CarteiraId", new List<string> { "ID da carteira inválido" });
        }
        else
        {
            var carteiraPertenceAoUsuario = await _carteiraRepository.UsuarioPossuiCarteiraAsync(usuarioId, request.CarteiraId);
            if (!carteiraPertenceAoUsuario)
            {
                errors.Add("CarteiraId", new List<string> { "Carteira não encontrada ou não pertence ao usuário" });
            }
        }

        // Validar ativo
        if (request.AtivoId <= 0)
        {
            errors.Add("AtivoId", new List<string> { "ID do ativo inválido" });
        }
        else
        {
            var ativoExiste = await _ativoRepository.ObterPorIdAsync(request.AtivoId);
            if (ativoExiste == null)
            {
                errors.Add("AtivoId", new List<string> { "Ativo não encontrado" });
            }
        }

        // Validar quantidade
        if (request.Quantidade == 0)
        {
            errors.Add("Quantidade", new List<string> { "Quantidade não pode ser zero" });
        }

        // Validar preço
        if (request.Preco <= 0)
        {
            errors.Add("Preco", new List<string> { "Preço deve ser maior que zero" });
        }

        // Validar tipo de transação
        if (string.IsNullOrWhiteSpace(request.TipoTransacao))
        {
            errors.Add("TipoTransacao", new List<string> { "Tipo de transação é obrigatório" });
        }
        else if (!TipoTransacao.EhValido(request.TipoTransacao))
        {
            errors.Add("TipoTransacao", new List<string> { $"Tipo de transação inválido. Valores permitidos: {string.Join(", ", TipoTransacao.TodosTipos)}" });
        }

        // Validar data da transação
        if (request.DataTransacao > DateTimeOffset.UtcNow)
        {
            errors.Add("DataTransacao", new List<string> { "Data da transação não pode ser no futuro" });
        }

        return errors;
    }

    private async Task<bool> VerificarSaldoSuficienteAsync(long carteiraId, long ativoId, decimal quantidadeVenda)
    {
        // Buscar todas as transações do ativo na carteira
        var transacoes = await _transacaoRepository.ObterPorCarteiraEAtivoAsync(carteiraId, ativoId);

        // Calcular saldo atual
        decimal saldoAtual = 0;
        foreach (var transacao in transacoes.OrderBy(t => t.DataTransacao))
        {
            if (transacao.TipoTransacao == TipoTransacao.Compra || transacao.TipoTransacao == TipoTransacao.Bonus)
            {
                saldoAtual += transacao.Quantidade;
            }
            else if (transacao.TipoTransacao == TipoTransacao.Venda)
            {
                saldoAtual -= transacao.Quantidade;
            }
            else if (transacao.TipoTransacao == TipoTransacao.Split)
            {
                // Exemplo: Split 2:1 significa multiplicar por 2
                // A quantidade na transação representa o fator
                saldoAtual *= transacao.Quantidade;
            }
            else if (transacao.TipoTransacao == TipoTransacao.Grupamento)
            {
                // Exemplo: Grupamento 10:1 significa dividir por 10
                // A quantidade na transação representa o fator
                saldoAtual /= transacao.Quantidade;
            }
        }

        return saldoAtual >= Math.Abs(quantidadeVenda);
    }
}
