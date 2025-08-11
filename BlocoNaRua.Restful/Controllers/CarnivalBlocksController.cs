using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Models.CarnivalBlock;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class CarnivalBlocksController(ICarnivalBlockService service) : ControllerBase
{
    private readonly ICarnivalBlockService _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _service.GetAllAsync();
        return Ok(list.Select(ToDTO).ToList());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null)
            return NotFound();
        var result = ToDTO(entity);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CarnivalBlockCreate model)
    {
        var entity = new CarnivalBlockEntity
        (
            id: 0,
            ownerId: model.OwnerId,
            name: model.Name,
            inviteCode: string.Empty,
            managersInviteCode: string.Empty,
            carnivalBlockImage: model.CarnivalBlockImage
        );
        var created = await _service.CreateAsync(entity);
        var result = ToDTO(created);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CarnivalBlockUpdate model, [FromHeader(Name = "X-Logged-Member")] int loggedMember)
    {
        var entity = new CarnivalBlockEntity
        (
            id: id,
            ownerId: 0,
            name: model.Name,
            inviteCode: string.Empty,
            managersInviteCode: string.Empty,
            carnivalBlockImage: model.CarnivalBlockImage
        );
        try
        {
            var updated = await _service.UpdateAsync(id, loggedMember, entity);
            if (updated is null)
                return NotFound();

            var result = ToDTO(updated);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, [FromHeader(Name = "X-Logged-Member")] int loggedMember)
    {
        try
        {
            await _service.DeleteAsync(id, loggedMember);
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
    }

    private static CarnivalBlockDTO ToDTO(CarnivalBlockEntity entity)
    {
        return new CarnivalBlockDTO(
            entity.Id,
            entity.OwnerId,
            entity.Name,
            entity.InviteCode,
            entity.ManagersInviteCode,
            entity.CarnivalBlockImage,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}
