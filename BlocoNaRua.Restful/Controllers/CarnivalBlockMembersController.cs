using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Models.CarnivalBlockMember;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class CarnivalBlockMembersController
    (
        ILogger<CarnivalBlockMembersController> logger,
        ICarnivalBlockMembersRepository carnivalBlockMembersRepo
    ) : ControllerBase
{
    private readonly ILogger<CarnivalBlockMembersController> _logger = logger;
    private readonly ICarnivalBlockMembersRepository _carnivalBlockMembersRepo = carnivalBlockMembersRepo;

    [HttpGet]
    public async Task<IActionResult> GetAllBlocksMembers()
    {
        var blocksMembersList = await _carnivalBlockMembersRepo.GetAllAsync();
        return Ok(blocksMembersList);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBlocksMembersById(int id)
    {
        var blockMember = await _carnivalBlockMembersRepo.GetByIdAsync(id);
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

            var result = await _carnivalBlockMembersRepo.AddAsync(entity);
            return CreatedAtAction
            (
                nameof(GetBlocksMembersById),
                new { id = result.Id },
                result
            );
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // [HttpPut("Update/{memberId}")] // TODO?

}
