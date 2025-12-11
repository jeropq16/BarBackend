using Barber.Domain.Enums;
using Barber.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Barber.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.Users.AnyAsync(u => u.Role == UserRole.Admin))
        {
            var super = new User
            {
                FullName = "admin",
                Email = "admin@barber.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), 
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(super);
            await db.SaveChangesAsync();
        }
    }
}