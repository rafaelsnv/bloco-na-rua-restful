using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Extensions;
using BlocoNaRua.Restful.Mappers;
using BlocoNaRua.Restful.Models.CarnivalBlockMember;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Authorize]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(List<CarnivalBlockMemberResponse>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public class CarnivalBlockMembersController(ICarnivalBlockMembersService carnivalBlockMembersService, IMemberIdentityService memberIdentityService) : ControllerBase
{
    private readonly ICarnivalBlockMembersService _carnivalBlockMembersService = carnivalBlockMembersService;
    private readonly IMemberIdentityService _memberIdentityService = memberIdentityService;

    [HttpGet]
    [ProducesResponseType(typeof(List<CarnivalBlockMemberResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllBlocksMembers()
    {
        var blocksMembersList = await _carnivalBlockMembersService.GetAllAsync();
        var response = blocksMembersList.Select(x => x.ToDTO());
        return Ok(response);
    }

    [HttpGet("block/{blockId}")]
    [ProducesResponseType(typeof(List<CarnivalBlockMemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBlocksMembersByBlockId(int blockId)
    {
        var blockMembers = await _carnivalBlockMembersService.GetByBlockIdAsync(blockId);
        if (blockMembers == null || !blockMembers.Any())
            return NotFound();
        var response = blockMembers.Select(x => x.ToDTO());
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CarnivalBlockMemberResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateCarnivalBlockMember([FromBody] CarnivalBlockMemberCreate blockMember)
    {
        try
        {
            if (blockMember == null)
                return BadRequest();

            var memberId = await _memberIdentityService.GetMemberIdAsync();

            var entity = new CarnivalBlockMembersEntity(
                id: 0,
                carnivalBlockId: blockMember.CarnivalBlockId,
                memberId: memberId,
                role: blockMember.Role
            );

            await _carnivalBlockMembersService.CreateAsync(entity, memberId);
            return CreatedAtAction
            (
                nameof(GetBlocksMembersByBlockId),
                new { blockId = entity.CarnivalBlockId },
                entity.ToDTO()
            );
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
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CarnivalBlockMemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateCarnivalBlockMember(int id, [FromBody] CarnivalBlockMemberUpdate updateRole)
    {
        try
        {
            if (updateRole == null)
                return BadRequest();

            var memberId = await _memberIdentityService.GetMemberIdAsync();

            var updated = await _carnivalBlockMembersService.UpdateAsync(id, memberId, updateRole.Role);
            if (updated == null)
                return NotFound();

            return Ok(updated.ToDTO());
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
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteCarnivalBlockMember(int id)
    {
        try
        {
            var memberId = await _memberIdentityService.GetMemberIdAsync();

            var deleted = await _carnivalBlockMembersService.DeleteAsync(id, memberId);
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
    }

}
