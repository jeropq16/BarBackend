using Barber.Application.DTOs.Users;
using Barber.Application.Interfaces;
using Barber.Domain.Enums;
using Barber.Domain.Interfaces;
using Barber.Domain.Models;

namespace Barber.Application.Services.Users;

public class UserService
{
    private readonly IUserRepository _userRepo;
    private readonly IFileStorageService  _fileStorageService;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository userRepo,  IFileStorageService fileStorageService,  IPasswordHasher passwordHasher)
    {
        _userRepo = userRepo;
        _fileStorageService = fileStorageService;
        _passwordHasher = passwordHasher;
    }

    public async Task<UsersProfileResponse?> GetByIdAsync(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);

        if (user is null) return null;

        return new UsersProfileResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            ProfilePhotoUrl = user.ProfilePhotoUrl
        };
    }
    
    public async Task<UsersProfileResponse> CreateUserAsync(CreateUserRequest request)
    {
        var exists = await _userRepo.GetByEmailAsync(request.Email);
        if (exists != null)
            throw new Exception("El usuario ya existe");

        var roleParsed = Enum.Parse<UserRole>(request.Role, true);

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = roleParsed,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _userRepo.AddAsync(user);

        return new UsersProfileResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        };
    }


    public async Task UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = await _userRepo.GetByIdAsync(id);

        if (user is null)
            throw new Exception("Usuario no encontrado.");

        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;

        await _userRepo.UpdateAsync(user);
    }
    
    public async Task<string> UpdatePhotoAsync(int userId, Stream fileStream, string fileName)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null)
            throw new Exception("Usuario no encontrado.");
        
        string url = await _fileStorageService.UploadAsync(fileStream, fileName);

        user.ProfilePhotoUrl = url;

        await _userRepo.UpdateAsync(user);

        return url;
    }

}