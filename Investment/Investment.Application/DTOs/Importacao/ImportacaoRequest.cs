using System.ComponentModel.DataAnnotations;

namespace Investment.Application.DTOs.Importacao;

public class ImportacaoRequest
{
    [Required(ErrorMessage = "CarteiraId é obrigatório")]
    public long CarteiraId { get; set; }

    [Required(ErrorMessage = "CorretoraTipo é obrigatório")]
    [RegularExpression("^(Clear|XP)$", ErrorMessage = "CorretoraTipo deve ser 'Clear' ou 'XP'")]
    public string CorretoraTipo { get; set; } = default!;
}
