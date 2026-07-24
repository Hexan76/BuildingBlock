using System.Security.Claims;

using Microsoft.AspNetCore.Http;

namespace Framework.BuildingBlock.Data;

public sealed class HttpCurrentUserAccessor
    : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;


    public HttpCurrentUserAccessor(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }


    public Guid? UserId
    {
        get
        {
            var user = _httpContextAccessor
                .HttpContext?
                .User;


            if (user?.Identity?.IsAuthenticated != true)
                return null;


            var userId =
                user.FindFirstValue(
                    ClaimTypes.NameIdentifier)
                ??
                user.FindFirstValue("sub")
                ??
                user.FindFirstValue("user_id")
                ??
                user.FindFirstValue("uid");


            return Guid.TryParse(userId, out var id)
                ? id
                : null;
        }
    }
}
