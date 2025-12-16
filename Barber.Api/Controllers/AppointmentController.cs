using System.Security.Claims;
using Barber.Application.DTOs.Appointments;
using Barber.Application.Services.Appointments;
using Barber.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Barber.Api.Controllers;

[ApiController]
[Route("appointments")]
public class AppointmentController : ControllerBase
{
    private readonly AppointmentService _appointmentService;

    public AppointmentController(AppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAppointmentRequest request)
    {
        var result = await _appointmentService.CreateAppointmentAsync(request);
        return Ok(result);
    }

    [HttpGet("availability")]
    public async Task<IActionResult> GetAvailability(int barberId, DateTime date, int haircutId)
    {
        var result = await _appointmentService.GetAvailabilityAsync(barberId, date, haircutId);
        return Ok(result);
    }
    
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateAppointmentRequest request)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        string role = User.FindFirst(ClaimTypes.Role)!.Value;

        var result = await _appointmentService.UpdateAppointmentAsync(id, userId, role, request);
        return Ok(result);
    }
    
    [HttpPut("payment-status")]
    public async Task<IActionResult> UpdatePaymentStatus(UpdatePaymentStatusRequest request)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        string role = User.FindFirst(ClaimTypes.Role)!.Value;

        var result = await _appointmentService.UpdatePaymentStatusAsync(userId, role, request);
        return Ok(result);
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        string role = User.FindFirst(ClaimTypes.Role)!.Value;

        await _appointmentService.CancelAppointmentAsync(id, userId, role);

        return NoContent();
    }
    
    [HttpPut("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        string role = User.FindFirst(ClaimTypes.Role)!.Value;

        var result = await _appointmentService.CompleteAppointmentAsync(id, userId, role);
        return Ok(result);
    }
    
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _appointmentService.GetAllPublicAsync();
        return Ok(result);
    }

    
    [HttpGet("filter")]
    public async Task<IActionResult> Filter([FromQuery] AppointmentFilterRequest filter)
    {
        var result = await _appointmentService.FilterAppointmentsPublicAsync(filter);
        return Ok(result);
    }

    
    

}