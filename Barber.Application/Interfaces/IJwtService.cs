using Barber.Domain.Models;

namespace Barber.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken (User user);
}