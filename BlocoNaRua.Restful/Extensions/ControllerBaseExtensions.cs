using System.Security.Claims;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Extensions;

public static class ControllerBaseExtensions
{
    /// <summary>
    /// ponytail: this exists — resolves memberId from the authenticated JWT sub claim via IMemberIdentityService.
    /// Add when throughput matters: cache the memberId lookup per request (HttpContext.Items).
    /// </summary>
    public static int GetMemberId(this ControllerBase controller, IMemberIdentityService memberIdentityService)
    {
        var subClaim = controller.HttpContext.User.FindFirst("sub");
        if (subClaim is null)
            throw new UnauthorizedAccessException("Missing 'sub' claim in JWT. Ensure the request includes a valid Bearer token.");

        try
        {
            var task = memberIdentityService.GetMemberIdAsync();
            return task.GetAwaiter().GetResult();
        }
        catch (AggregateException ex) when (ex.InnerException is UnauthorizedAccessException)
        {
            throw ex.InnerException;
        }
    }
}
