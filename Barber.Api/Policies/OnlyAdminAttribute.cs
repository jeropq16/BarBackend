using Microsoft.AspNetCore.Authorization;

namespace Barber.Api.Policies;

public class OnlyAdminAttribute : AuthorizeAttribute
{
    public OnlyAdminAttribute()
    {
        Policy = "AdminOnly";
    }
}