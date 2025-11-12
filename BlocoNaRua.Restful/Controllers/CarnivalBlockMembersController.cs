using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Models.CarnivalBlockMember;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class CarnivalBlockMembersController
    (
        ILogger<CarnivalBlockMembersController> logger,
        ICarnivalBlockMembersService carnivalBlockMembersService
    ) : ControllerBase
{
    private readonly ILogger<CarnivalBlockMembersController> _logger = logger;
    private readonly ICarnivalBlockMembersService _carnivalBlockMembersService = carnivalBlockMembersService;

    [HttpGet]
    public async Task<IActionResult> GetAllBlocksMembers()
    {
        var blocksMembersList = await _carnivalBlockMembersService.GetAllAsync();
        var response = blocksMembersList.Select(ToDTO).ToList();
        return Ok(response);
    }

    [HttpGet("block/{blockId}")]
    public async Task<IActionResult> GetBlocksMembersByBlockId(int blockId)
    {
        var blockMembers = await _carnivalBlockMembersService.GetByBlockIdAsync(blockId);
        if (blockMembers == null || !blockMembers.Any())
            return NotFound();
        var response = blockMembers.Select(ToDTO).ToList();
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCarnivalBlockMember([FromBody] CarnivalBlockMemberCreate blockMember, [FromHeader(Name = "X-Logged-Member")] int loggedMember)
    {
        try
        {
            if (blockMember == null)
                return BadRequest();

            var entity = new CarnivalBlockMembersEntity(
                id: 0,
                carnivalBlockId: blockMember.CarnivalBlockId,
                memberId: blockMember.MemberId,
                role: blockMember.Role
            );

            await _carnivalBlockMembersService.CreateAsync(entity, loggedMember);
            return CreatedAtAction
            (
                nameof(GetBlocksMembersByBlockId),
                new { blockId = entity.CarnivalBlockId },
                ToDTO(entity)
            );
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCarnivalBlockMember(int id, [FromBody] CarnivalBlockMemberUpdate updateRole, [FromHeader(Name = "X-Logged-Member")] int loggedMember)
    {
        try
        {
            if (updateRole == null)
                return BadRequest();

            var updated = await _carnivalBlockMembersService.UpdateAsync(id, loggedMember, updateRole.Role);
            if (updated == null)
                return NotFound();

            return Ok(ToDTO(updated));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCarnivalBlockMember(int id, [FromHeader(Name = "X-Logged-Member")] int loggedMember)
    {
        try
        {
            var deleted = await _carnivalBlockMembersService.DeleteAsync(id, loggedMember);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private static CarnivalBlockMemberDTO ToDTO(CarnivalBlockMembersEntity entity)
    {
        return new CarnivalBlockMemberDTO(
            entity.Id,
            entity.CarnivalBlockId,
            entity.MemberId,
            entity.Role,
            entity.CreatedAt.GetValueOrDefault(),
            entity.UpdatedAt.GetValueOrDefault()
        );
    }

}
