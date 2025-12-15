using System.Security.Claims;
using Barber.Api.Policies;
using Barber.Application.DTOs.Users;
using Barber.Application.Interfaces;
using Barber.Application.Services.Users;
using Barber.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Barber.Api.Controllers;


[ApiController]
[Route("users")]
public class UsersController  : ControllerBase
{
    private readonly UserService _userService;
    private readonly IFileStorageService _fileStorage;

    public UsersController(UserService userService, IFileStorageService fileStorage)
    {
        _userService = userService;
        _fileStorage = fileStorage;
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        int UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var user = await _userService.GetByIdAsync(UserId);
        return Ok(user);
    }
    
    [HttpPost("create-staff")]

    public async Task<IActionResult> CreateStaff(CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(request);
        return Ok(user);
    }

    [HttpGet("{id :int}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        return Ok(user);
    }

    [HttpPut("{id :int}")]
    public async Task<IActionResult> Update(int id, UpdateUserRequest request)
    {
        await _userService.UpdateAsync(id, request);
        return NoContent();
    }

    [HttpPut("{id:int}/upload-photo")]
    public async Task<IActionResult> UploadPhoto(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Archivo no válido.");

        using var stream = file.OpenReadStream();

        string url = await _userService.UpdatePhotoAsync(id, stream, file.FileName);

        return Ok(new { url });
    }
    
    [HttpPost("barbers")]
    public async Task<IActionResult> CreateBarber(CreateBarberRequest request)
    {
        var barber = await _userService.CreateBarberAsync(request);
        return Ok(barber);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }
    
    [HttpGet("barbers")]
    public async Task<IActionResult> GetBarbers()
    {
        var barbers = await _userService.GetBarbersAsync();
        return Ok(barbers);
    }


}