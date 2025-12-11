using System.ComponentModel.DataAnnotations;

namespace Investment.Application.DTOs.Carteira;

public class CarteiraRequest
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres")]
    public string Nome { get; set; } = default!;

    [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
    public string? Descricao { get; set; }
}
