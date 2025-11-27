using System.ComponentModel.DataAnnotations;

namespace Investment.Application.DTOs;

public class AtivoRequest
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 200 caracteres")]
    public string Nome { get; set; } = default!;

    [Required(ErrorMessage = "O código é obrigatório")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "O código deve ter entre 2 e 20 caracteres")]
    [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "O código deve conter apenas letras maiúsculas e números")]
    public string Codigo { get; set; } = default!;

    [Required(ErrorMessage = "O tipo é obrigatório")]
    [StringLength(50, ErrorMessage = "O tipo deve ter no máximo 50 caracteres")]
    public string Tipo { get; set; } = default!;

    [StringLength(1000, ErrorMessage = "A descrição deve ter no máximo 1000 caracteres")]
    public string? Descricao { get; set; }
}
