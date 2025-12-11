namespace Investment.Application.Services.Calculos;

/// <summary>
/// Calculadora de TWR (Time-Weighted Return)
/// Elimina o efeito do timing de aportes e resgates
/// </summary>
public static class TwrCalculator
{
    /// <summary>
    /// Calcula o TWR (Time-Weighted Return) do período
    /// </summary>
    /// <param name="valorInicial">Valor no início do período</param>
    /// <param name="valorFinal">Valor no final do período</param>
    /// <param name="fluxos">Lista de fluxos (aportes positivos, resgates negativos)</param>
    /// <returns>TWR em percentual ou null se dados insuficientes</returns>
    public static decimal? Calculate(
        decimal valorInicial,
        decimal valorFinal,
        List<(DateTime Data, decimal Aportes, decimal Resgates)> fluxos)
    {
        if (valorInicial <= 0)
            return null;

        // Se não há fluxos, cálculo simples
        if (fluxos == null || fluxos.Count == 0)
        {
            return ((valorFinal - valorInicial) / valorInicial) * 100m;
        }

        // Ordenar fluxos por data
        var fluxosOrdenados = fluxos
            .Where(f => f.Aportes != 0 || f.Resgates != 0)
            .OrderBy(f => f.Data)
            .ToList();

        if (fluxosOrdenados.Count == 0)
        {
            return ((valorFinal - valorInicial) / valorInicial) * 100m;
        }

        // TWR = Π ((Ending_Value + Withdrawals) / (Beginning_Value + Deposits)) - 1
        // Calculamos para cada sub-período entre fluxos

        decimal produto = 1m;
        decimal saldoAtual = valorInicial;

        foreach (var fluxo in fluxosOrdenados)
        {
            // Valor antes do fluxo (precisaria de dados históricos reais)
            // Como simplificação, assumimos valoração linear
            var aporteTotal = fluxo.Aportes;
            var resgateTotal = fluxo.Resgates;

            if (saldoAtual <= 0)
                return null;

            // Retorno do sub-período: (Valor_Antes_Retirada + Retirada) / (Valor_Inicio + Deposito)
            // Simplificação: (Saldo + Aporte - Resgate) / Saldo
            var saldoAposFluxo = saldoAtual + aporteTotal - resgateTotal;

            if (aporteTotal > 0)
            {
                // Período com aporte: retorno = saldo_depois / (saldo_antes + aporte)
                var retornoPeriodo = saldoAposFluxo / (saldoAtual + aporteTotal);
                produto *= retornoPeriodo;
            }
            else if (resgateTotal > 0)
            {
                // Período com resgate: retorno = (saldo_depois + resgate) / saldo_antes
                var retornoPeriodo = (saldoAposFluxo + resgateTotal) / saldoAtual;
                produto *= retornoPeriodo;
            }

            saldoAtual = saldoAposFluxo;
        }

        // Aplicar retorno final até o valor final
        if (saldoAtual > 0)
        {
            var retornoFinal = valorFinal / saldoAtual;
            produto *= retornoFinal;
        }

        // Converter para percentual
        var twr = (produto - 1m) * 100m;

        return twr;
    }

    /// <summary>
    /// Versão simplificada do TWR quando não há dados detalhados de fluxos
    /// </summary>
    public static decimal CalculateSimple(
        decimal valorInicial,
        decimal valorFinal,
        decimal totalAportes,
        decimal totalResgates)
    {
        if (valorInicial <= 0)
            return 0m;

        // Aproximação: TWR ≈ (ValorFinal + Resgates - Aportes - ValorInicial) / ValorInicial
        var retorno = (valorFinal + totalResgates - totalAportes - valorInicial) / valorInicial;
        return retorno * 100m;
    }
}
