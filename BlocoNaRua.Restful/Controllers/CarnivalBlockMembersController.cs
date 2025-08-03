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
        return Ok(blocksMembersList);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBlocksMembersById(int id)
    {
        var blockMember = await _carnivalBlockMembersService.GetByIdAsync(id);
        if (blockMember == null)
            return NotFound();
        return Ok(blockMember);
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
                entity
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
}
