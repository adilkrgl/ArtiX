namespace ArtiX.Api.Dtos.Auth;

public class LoginRequestDto
{
    public string UserNameOrEmail { get; set; } = null!;

    public string Password { get; set; } = null!;
}
