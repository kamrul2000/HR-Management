using System.Security.Claims;

namespace HRM.API.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static int GetSubscriptionId(this ClaimsPrincipal? principal)
    {
        if (principal is null)
        {
            throw new UnauthorizedAccessException("No authenticated user on the request.");
        }

        var raw = principal.FindFirst("subscriptionId")?.Value;
        if (string.IsNullOrWhiteSpace(raw) || !int.TryParse(raw, out var subscriptionId))
        {
            throw new UnauthorizedAccessException("Authenticated user is missing a valid subscriptionId claim.");
        }

        return subscriptionId;
    }

    public static int GetUserId(this ClaimsPrincipal? principal)
    {
        if (principal is null)
        {
            throw new UnauthorizedAccessException("No authenticated user on the request.");
        }

        var raw = principal.FindFirst("userId")?.Value;
        if (string.IsNullOrWhiteSpace(raw) || !int.TryParse(raw, out var userId))
        {
            throw new UnauthorizedAccessException("Authenticated user is missing a valid userId claim.");
        }

        return userId;
    }
}
