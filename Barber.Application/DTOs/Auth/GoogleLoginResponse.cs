namespace Barber.Application.DTOs.Auth;

public class GoogleLoginResponse
{
    public string Token { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
}