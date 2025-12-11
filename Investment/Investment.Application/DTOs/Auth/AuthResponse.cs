namespace Investment.Application.DTOs.Auth;

public class AuthResponse
{
    public string Token { get; set; } = default!;
    public DateTimeOffset ExpiresAt { get; set; }
    public UsuarioResponse Usuario { get; set; } = default!;
}

public class UsuarioResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateTimeOffset CriadoEm { get; set; }
}
