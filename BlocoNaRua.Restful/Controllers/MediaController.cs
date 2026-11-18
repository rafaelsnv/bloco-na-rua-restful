using Asp.Versioning;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Authorize]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class MediaController(
    IImageUrlService imageUrlService,
    IMemberIdentityService memberIdentityService,
    IMembersService membersService) : ControllerBase
{
    private readonly IImageUrlService _imageUrlService = imageUrlService;
    private readonly IMemberIdentityService _memberIdentityService = memberIdentityService;
    private readonly IMembersService _membersService = membersService;

    [HttpPost("image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    // No [FromForm] here: Swashbuckle cannot generate an operation for IFormFile with an
    // explicit [FromForm] attribute, and [ApiController] already infers multipart binding.
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file provided" });

        int memberId;
        try
        {
            memberId = await _memberIdentityService.GetMemberIdAsync();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }

        var member = await _membersService.GetByIdAsync(memberId);
        if (member is null)
            return Unauthorized(new { message = "Member not found" });

        string savedPath;
        try
        {
            using var stream = file.OpenReadStream();
            savedPath = await _imageUrlService.SaveImageAsync(stream, file.FileName);
        }
        catch (NotSupportedException ex)
        {
            return StatusCode(StatusCodes.Status415UnsupportedMediaType, new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return StatusCode(StatusCodes.Status413PayloadTooLarge, new { message = ex.Message });
        }

        // Only local uploads are managed here; external/legacy paths are left untouched.
        if (!string.IsNullOrWhiteSpace(member.ProfileImage) && member.ProfileImage.StartsWith("/uploads/"))
            await _imageUrlService.DeleteImageAsync(member.ProfileImage);

        member.ProfileImage = savedPath;
        await _membersService.UpdateAsync(member.Id, memberId, member);

        return Ok(new { path = savedPath });
    }
}
