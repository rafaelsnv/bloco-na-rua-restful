using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Mappers;
using BlocoNaRua.Restful.Models.Meeting;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class MeetingsController(IMeetingService service) : ControllerBase
{
    private readonly IMeetingService _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _service.GetAllAsync();
        return Ok(list.Select(MeetingMapper.ToDTO).ToList());
    }

    [HttpGet("block/{blockId}")]
    public async Task<IActionResult> GetAllByBlockId(int blockId)
    {
        var list = await _service.GetAllByBlockIdAsync(blockId);
        if (list == null || !list.Any())
            return NotFound();
        return Ok(list.Select(MeetingMapper.ToDTO).ToList());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MeetingCreate model, [FromHeader(Name = "X-Logged-Member")] int loggedMember)
    {
        var entity = new MeetingEntity
        (
            id: 0,
            name: model.Name,
            description: model.Description,
            location: model.Location,
            meetingCode: string.Empty,
            meetingDateTime: model.MeetingDateTime,
            carnivalBlockId: model.CarnivalBlockId
        );
        try
        {
            var created = await _service.CreateAsync(entity, loggedMember);
            var result = MeetingMapper.ToDTO(created);
            return CreatedAtAction(nameof(GetAllByBlockId), new { blockId = result.CarnivalBlockId }, result);
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
    public async Task<IActionResult> Update(int id, [FromBody] MeetingUpdate model, [FromHeader(Name = "X-Logged-Member")] int loggedMember)
    {
        var entity = new MeetingEntity
        (
            id: 0,
            name: model.Name,
            description: model.Description,
            location: model.Location,
            meetingCode: string.Empty,
            meetingDateTime: model.MeetingDateTime,
            carnivalBlockId: 0
        );
        try
        {
            var updated = await _service.UpdateAsync(id, entity, loggedMember);
            if (updated is null)
                return NotFound();

            var result = MeetingMapper.ToDTO(updated);
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

}
