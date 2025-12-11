using System.ComponentModel.DataAnnotations;

namespace Investment.Application.DTOs.Transacao;

public class TransacaoRequest
{
    [Required(ErrorMessage = "O ID da carteira é obrigatório")]
    public long CarteiraId { get; set; }

    [Required(ErrorMessage = "O ID do ativo é obrigatório")]
    public long AtivoId { get; set; }

    [Required(ErrorMessage = "A quantidade é obrigatória")]
    public decimal Quantidade { get; set; }

    [Required(ErrorMessage = "O preço é obrigatório")]
    [Range(0.0001, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
    public decimal Preco { get; set; }

    [Required(ErrorMessage = "O tipo de transação é obrigatório")]
    [StringLength(50, ErrorMessage = "O tipo de transação deve ter no máximo 50 caracteres")]
    public string TipoTransacao { get; set; } = default!;

    [Required(ErrorMessage = "A data da transação é obrigatória")]
    public DateTimeOffset DataTransacao { get; set; }
}
