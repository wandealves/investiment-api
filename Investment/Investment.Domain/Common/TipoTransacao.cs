namespace Investment.Domain.Common;

public static class TipoTransacao
{
    public const string Compra = "Compra";
    public const string Venda = "Venda";
    public const string Dividendo = "Dividendo";
    public const string JCP = "JCP";
    public const string Bonus = "Bonus";
    public const string Split = "Split";
    public const string Grupamento = "Grupamento";

    public static readonly string[] TodosTipos =
    {
        Compra,
        Venda,
        Dividendo,
        JCP,
        Bonus,
        Split,
        Grupamento
    };

    public static bool EhValido(string tipo)
    {
        return TodosTipos.Contains(tipo);
    }
}
