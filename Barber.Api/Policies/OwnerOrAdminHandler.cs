using System.Security.Claims;
using Barber.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Barber.Api.Policies;

public class OwnerOrAdminHandler : AuthorizationHandler<OwnerOrAdminRequirement>
{
    private readonly IAppointmentRepository _appointmentRepo;

    public OwnerOrAdminHandler(IAppointmentRepository appointmentRepo)
    {
        _appointmentRepo = appointmentRepo;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnerOrAdminRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        var roleClaim = context.User.FindFirst(ClaimTypes.Role);

        if (userIdClaim == null || roleClaim == null)
        {
            context.Fail();
            return;
        }

        int userId = int.Parse(userIdClaim.Value);
        string role = roleClaim.Value;

        // Admin 
        if (role == "Admin")
        {
            context.Succeed(requirement);
            return;
        }

        // Asegurarnos de que el recurso es un AuthorizationFilterContext (MVC)
        if (context.Resource is not AuthorizationFilterContext mvcContext)
        {
            context.Fail();
            return;
        }

        // Obtener route values
        var routeValues = mvcContext.RouteData.Values;

        if (!routeValues.TryGetValue("id", out var rawId) || rawId is null)
        {
            context.Fail();
            return;
        }

        if (!int.TryParse(rawId.ToString(), out var appointmentId))
        {
            context.Fail();
            return;
        }

        var appointment = await _appointmentRepo.GetByIdAsync(appointmentId);

        if (appointment == null)
        {
            context.Fail();
            return;
        }

        // Dueño (cliente)
        if (appointment.ClientId == userId)
        {
            context.Succeed(requirement);
            return;
        }

        // Barbero asignado
        if (appointment.BarberId == userId)
        {
            context.Succeed(requirement);
            return;
        }

        
        context.Fail();
    }
}
