using System.ComponentModel.DataAnnotations;
using Investment.Domain.Common;

namespace Investment.Application.DTOs.Provento;

public class ProventoRequest
{
    [Required(ErrorMessage = "O ID do ativo é obrigatório")]
    public long AtivoId { get; set; }

    [Required(ErrorMessage = "O tipo de provento é obrigatório")]
    public TipoProvento TipoProvento { get; set; }

    [Required(ErrorMessage = "O valor por cota é obrigatório")]
    [Range(0.0001, (double)decimal.MaxValue, ErrorMessage = "O valor por cota deve ser maior que zero")]
    public decimal ValorPorCota { get; set; }

    [Required(ErrorMessage = "A data COM é obrigatória")]
    public DateTimeOffset DataCom { get; set; }

    public DateTimeOffset? DataEx { get; set; }

    [Required(ErrorMessage = "A data de pagamento é obrigatória")]
    public DateTimeOffset DataPagamento { get; set; }

    [Required(ErrorMessage = "O status do provento é obrigatório")]
    public StatusProvento Status { get; set; }

    [StringLength(500, ErrorMessage = "A observação deve ter no máximo 500 caracteres")]
    public string? Observacao { get; set; }
}
