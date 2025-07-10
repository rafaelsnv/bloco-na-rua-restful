using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Models.CarnivalBlock;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarnivalBlocksController
    (
        ILogger<CarnivalBlocksController> logger,
        ICarnivalBlocksRepository carnivalBlocksRepository
    ) : ControllerBase
{
    private readonly ILogger<CarnivalBlocksController> _logger = logger;
    private readonly ICarnivalBlocksRepository _carnivalBlocksRepository = carnivalBlocksRepository;

    [HttpGet("Get")]
    public async Task<IActionResult> GetAllCarnivalBlocks()
    {
        var carnivalBlocksList = await _carnivalBlocksRepository.GetAllAsync();
        return Ok(carnivalBlocksList);
    }

    [HttpGet("Get/{id}")]
    public async Task<IActionResult> GetCarnivalBlockById(int id)
    {
        var carnivalBlock = await _carnivalBlocksRepository.GetByIdAsync(id);
        if (carnivalBlock == null)
            return NotFound();
        return Ok(carnivalBlock);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> CreateCarnivalBlock([FromBody] CarnivalBlockCreate carnivalBlock)
    {
        if (carnivalBlock == null)
            return BadRequest();

        var entity = new CarnivalBlockEntity(
            id: 0,
            name: carnivalBlock.Name,
            owner: carnivalBlock.Owner,
            inviteCode: string.Empty,
            managersInviteCode: string.Empty,
            carnivalBlockImage: carnivalBlock.CarnivalBlockImage
        );

        var result = await _carnivalBlocksRepository.AddAsync(entity);
        return CreatedAtAction
        (
            nameof(GetCarnivalBlockById),
            new { id = result.Id },
            result
        );
    }

    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> DeleteCarnivalBlock(int id)
    {
        var member = await _carnivalBlocksRepository.GetByIdAsync(id);
        if (member == null)
            return NotFound();

        await _carnivalBlocksRepository.DeleteAsync(member);
        return NoContent();
    }
}
