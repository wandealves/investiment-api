using System.ComponentModel.DataAnnotations;

namespace Investment.Application.DTOs.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 150 caracteres")]
    public string Nome { get; set; } = default!;

    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "O email deve ser válido")]
    [StringLength(200, ErrorMessage = "O email deve ter no máximo 200 caracteres")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "A senha é obrigatória")]
    [StringLength(500, MinimumLength = 8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres")]
    public string Senha { get; set; } = default!;
}
