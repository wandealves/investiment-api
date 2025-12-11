using System.ComponentModel.DataAnnotations;

namespace Investment.Application.DTOs.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "O email deve ser válido")]
    [StringLength(200, ErrorMessage = "O email deve ter no máximo 200 caracteres")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "A senha é obrigatória")]
    [StringLength(500, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
    public string Senha { get; set; } = default!;
}
