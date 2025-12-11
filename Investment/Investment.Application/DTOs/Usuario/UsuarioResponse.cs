namespace Investment.Application.DTOs.Usuario;

public class UsuarioResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateTimeOffset CriadoEm { get; set; }
}
