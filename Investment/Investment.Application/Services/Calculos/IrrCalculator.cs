namespace Investment.Application.Services.Calculos;

/// <summary>
/// Calculadora de IRR (Internal Rate of Return) usando método Newton-Raphson
/// </summary>
public static class IrrCalculator
{
    private const int MaxIterations = 100;
    private const decimal Tolerance = 0.0001m;
    private const decimal InitialGuess = 0.1m; // 10% como palpite inicial

    /// <summary>
    /// Calcula o IRR (Taxa Interna de Retorno) anualizada
    /// </summary>
    /// <param name="cashFlows">Lista de fluxos de caixa com data e valor</param>
    /// <returns>IRR em percentual (ex: 15.5 para 15.5%) ou null se não convergir</returns>
    public static decimal? Calculate(List<(DateTime Data, decimal Valor)> cashFlows)
    {
        if (cashFlows == null || cashFlows.Count < 2)
            return null;

        // Ordenar por data
        var fluxosOrdenados = cashFlows.OrderBy(cf => cf.Data).ToList();
        var dataInicial = fluxosOrdenados.First().Data;

        // Verificar se há fluxos positivos e negativos
        var temPositivo = fluxosOrdenados.Any(cf => cf.Valor > 0);
        var temNegativo = fluxosOrdenados.Any(cf => cf.Valor < 0);

        if (!temPositivo || !temNegativo)
            return null;

        // Método Newton-Raphson
        decimal irr = InitialGuess;

        for (int i = 0; i < MaxIterations; i++)
        {
            var npv = CalculateNPV(fluxosOrdenados, dataInicial, irr);
            var derivative = CalculateNPVDerivative(fluxosOrdenados, dataInicial, irr);

            if (Math.Abs(derivative) < 0.00001m)
                break;

            var irrNovo = irr - (npv / derivative);

            if (Math.Abs(irrNovo - irr) < Tolerance)
            {
                // Converter para percentual anualizado
                return irrNovo * 100m;
            }

            irr = irrNovo;

            // Evitar valores muito extremos
            if (irr < -0.99m || irr > 10m)
                return null;
        }

        return null; // Não convergiu
    }

    /// <summary>
    /// Calcula o NPV (Net Present Value) para um dado IRR
    /// NPV = Σ (CashFlow_i / (1 + IRR)^((Date_i - Date_0) / 365))
    /// </summary>
    private static decimal CalculateNPV(
        List<(DateTime Data, decimal Valor)> cashFlows,
        DateTime dataInicial,
        decimal irr)
    {
        decimal npv = 0m;

        foreach (var cf in cashFlows)
        {
            var dias = (cf.Data - dataInicial).TotalDays;
            var anos = (decimal)dias / 365m;
            var fator = (decimal)Math.Pow((double)(1 + irr), (double)anos);

            npv += cf.Valor / fator;
        }

        return npv;
    }

    /// <summary>
    /// Calcula a derivada do NPV em relação ao IRR
    /// dNPV/dIRR = Σ (-anos * CashFlow_i / (1 + IRR)^(anos + 1))
    /// </summary>
    private static decimal CalculateNPVDerivative(
        List<(DateTime Data, decimal Valor)> cashFlows,
        DateTime dataInicial,
        decimal irr)
    {
        decimal derivative = 0m;

        foreach (var cf in cashFlows)
        {
            var dias = (cf.Data - dataInicial).TotalDays;
            var anos = (decimal)dias / 365m;
            var fator = (decimal)Math.Pow((double)(1 + irr), (double)(anos + 1));

            derivative += -anos * cf.Valor / fator;
        }

        return derivative;
    }
}
