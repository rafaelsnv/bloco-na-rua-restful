using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Mappers;
using BlocoNaRua.Restful.Models.MeetingPresence;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(List<MeetingPresenceResponse>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public class MeetingPresencesController(IMeetingPresenceService service) : ControllerBase
{
    private readonly IMeetingPresenceService _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(List<MeetingPresenceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int? page = null, [FromQuery] int? pageSize = null)
    {
        var list = await _service.GetAllAsync(page, pageSize);
        return Ok(list.Select(MeetingPresenceMapper.ToDTO).ToList());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MeetingPresenceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null)
            return NotFound();
        var result = MeetingPresenceMapper.ToDTO(entity);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(MeetingPresenceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
            var result = MeetingPresenceMapper.ToDTO(created);
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
    [ProducesResponseType(typeof(MeetingPresenceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
            var result = MeetingPresenceMapper.ToDTO(updated);
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

}
