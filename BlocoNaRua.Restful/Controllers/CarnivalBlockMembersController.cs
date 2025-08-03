using BlocoNaRua.Data.Repositories.Interfaces;
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBlocksMembersById(int id)
    {
        var blockMember = await _carnivalBlockMembersService.GetByIdAsync(id);
        if (blockMember == null)
            return NotFound();
        var response = ToDTO(blockMember);
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCarnivalBlockMember([FromBody] CarnivalBlockMemberCreate blockMember)
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

            await _carnivalBlockMembersService.CreateAsync(entity);
            return CreatedAtAction
            (
                nameof(GetBlocksMembersById),
                new { id = entity.Id },
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
    public async Task<IActionResult> UpdateCarnivalBlockMember(int id, [FromBody] CarnivalBlockMemberUpdate updateRole)
    {
        try
        {
            if (updateRole == null)
                return BadRequest();

            var updated = await _carnivalBlockMembersService.UpdateAsync(id, updateRole.LoggedMemberId, updateRole.Role);
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
    public async Task<IActionResult> DeleteCarnivalBlockMember(int id, [FromHeader(Name = "X-Member-Id")] int loggedMemberId)
    {
        try
        {
            var deleted = await _carnivalBlockMembersService.DeleteAsync(id, loggedMemberId);
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
