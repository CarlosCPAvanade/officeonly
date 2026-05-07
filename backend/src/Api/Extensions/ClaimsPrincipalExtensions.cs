using System.Security.Claims;
using Application.Exceptions;

namespace Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetRequiredUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue(ClaimTypes.Name)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");

        if (!Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedException("Token inválido.");
        }

        return userId;
    }

    public static string GetRequiredRole(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Role) ?? throw new UnauthorizedException("Rol no encontrado en el token.");
    }
}
