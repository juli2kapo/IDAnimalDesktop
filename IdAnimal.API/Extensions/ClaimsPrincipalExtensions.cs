using System.Security.Claims;

namespace IdAnimal.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(userIdClaim, out int userId))
        {
            return userId;
        }

        throw new UnauthorizedAccessException("Invalid token: User ID missing or invalid.");
    }
}