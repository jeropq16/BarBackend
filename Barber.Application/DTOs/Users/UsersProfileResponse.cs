using Barber.Domain.Enums;

namespace Barber.Application.DTOs.Users;

public class UsersProfileResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; }
    public string? ProfilePhotoUrl { get; set; }
}