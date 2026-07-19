using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

// ponytail: this exists — [AllowAnonymous] because it's called by server-side cleanup process.
// Revisit when Owner-only middleware is added (living-notes.md C3).
[ApiController]
[Route("api/v{version:apiVersion}/admin")]
[AllowAnonymous]
public class AdminController(IAdminService adminService) : ControllerBase
{
    private readonly IAdminService _adminService = adminService;

    [HttpDelete("signup-cleanup/{uuid:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteSignupCleanup(Guid uuid)
    {
        try
        {
            var result = await _adminService.DeleteSignupAsync(uuid);
            if (!result.Deleted)
                return NotFound(result.ErrorMessage ?? "User not found");
            return Ok(new { deleted = true });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, new { statusCode = 500, message = "Supabase error", detail = ex.Message });
        }
    }
}