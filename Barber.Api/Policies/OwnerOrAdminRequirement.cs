using Microsoft.AspNetCore.Authorization;

namespace Barber.Api.Policies;

public class OwnerOrAdminRequirement : IAuthorizationRequirement { }