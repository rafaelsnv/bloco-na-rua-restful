using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Models.MeetingPresence;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class MeetingPresencesController(IMeetingPresenceService service) : ControllerBase
{
    private readonly IMeetingPresenceService _service = service;

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
    public async Task<IActionResult> Create([FromBody] MeetingPresenceCreate model, [FromHeader(Name = "X-Logged-Member")] int loggedMember)
    {
        var entity = new MeetingPresenceEntity(0)
        {
            MemberId = model.MemberId,
            MeetingId = model.MeetingId,
            CarnivalBlockId = model.CarnivalBlockId,
            IsPresent = model.IsPresent
        };
        try
        {
            var created = await _service.CreateAsync(entity, loggedMember);
            var result = ToDTO(created);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] MeetingPresenceUpdate model, [FromHeader(Name = "X-Logged-Member")] int loggedMember)
    {
        var entity = new MeetingPresenceEntity(0)
        {
            IsPresent = model.IsPresent
        };
        try
        {
            var updated = await _service.UpdateAsync(id, entity, loggedMember);
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

    private static MeetingPresenceDTO ToDTO(MeetingPresenceEntity entity)
    {
        return new MeetingPresenceDTO(
            entity.Id,
            entity.MemberId,
            entity.MeetingId,
            entity.CarnivalBlockId,
            entity.IsPresent,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}
