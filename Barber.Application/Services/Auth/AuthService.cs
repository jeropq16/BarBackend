using Barber.Application.DTOs.Auth;
using Barber.Application.Interfaces;
using Barber.Domain.Enums;
using Barber.Domain.Interfaces;
using Barber.Domain.Models;
using Google.Apis.Auth;

namespace Barber.Application.Services.Auth;

public class AuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthService(
        IUserRepository userRepo,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _userRepo = userRepo;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email);

        if (user is null)
            throw new Exception("Usuario no encontrado.");

        if (!_passwordHasher.Verify(user.PasswordHash!, request.Password))
            throw new Exception("Contraseña incorrecta.");

        return new LoginResponse
        {
            FullName = user.FullName,
            Role = user.Role.ToString(),
            Token = _jwtService.GenerateToken(user)
        };
    }

    public async Task<GoogleLoginResponse> LoginWithGoogleAsync(GoogleLoginRequest request, string googleClientId)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(
            request.GoogleToken,
            new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new List<string> { googleClientId }
            });
    
        string email = payload.Email;
        string fullName = payload.Name;
        string? photo = payload.Picture;

        var user = await _userRepo.GetByEmailAsync(email);

        if (user == null)
        {
            user = new User
            {
                FullName = fullName,
                Email = email,
                Role = UserRole.Client,
                ProfilePhotoUrl = photo,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.AddAsync(user);
        }

        var token = _jwtService.GenerateToken(user);

        return new GoogleLoginResponse
        {
            Token = token,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }
    
    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        // Validar que el email no exista
        var existing = await _userRepo.GetByEmailAsync(request.Email);
        if (existing != null)
            throw new Exception("Ya existe un usuario registrado con este correo.");

        // Crear usuario
        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = UserRole.Client,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.AddAsync(user);

        // Token inmediato (inicio de sesión automático)
        var token = _jwtService.GenerateToken(user);

        return new RegisterResponse
        {
            Token = token,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

        
}
