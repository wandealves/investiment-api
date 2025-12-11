using System.ComponentModel.DataAnnotations;

namespace Investment.Application.DTOs.Usuario;

public class UsuarioRequest
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 150 caracteres")]
    public string Nome { get; set; } = default!;

    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "O email deve ser válido")]
    [StringLength(200, ErrorMessage = "O email deve ter no máximo 200 caracteres")]
    public string Email { get; set; } = default!;
}
