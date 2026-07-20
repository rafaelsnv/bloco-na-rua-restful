using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Extensions;
using BlocoNaRua.Restful.Mappers;
using BlocoNaRua.Restful.Models.Meeting;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Authorize]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(List<MeetingResponse>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public class MeetingsController(IMeetingService service, IMemberIdentityService memberIdentityService) : ControllerBase
{
    private readonly IMeetingService _service = service;
    private readonly IMemberIdentityService _memberIdentityService = memberIdentityService;

    [HttpGet]
    [ProducesResponseType(typeof(List<MeetingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int? page = null, [FromQuery] int? pageSize = null)
    {
        var list = await _service.GetAllAsync(page, pageSize);
        return Ok(list.Select(x => x.ToDTO()).ToList());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MeetingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null)
            return NotFound();
        var result = entity.ToDTO();
        return Ok(result);
    }

    [HttpGet("block/{blockId}")]
    [ProducesResponseType(typeof(List<MeetingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllByBlockId(int blockId)
    {
        var list = await _service.GetAllByBlockIdAsync(blockId);
        if (list == null || !list.Any())
            return NotFound();
        return Ok(list.Select(x => x.ToDTO()).ToList());
    }

    [HttpPost]
    [ProducesResponseType(typeof(MeetingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] MeetingCreate model)
    {
        var memberId = await _memberIdentityService.GetMemberIdAsync();
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
            var created = await _service.CreateAsync(entity, memberId);
            var result = created.ToDTO();
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
    [ProducesResponseType(typeof(MeetingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(int id, [FromBody] MeetingUpdate model)
    {
        var memberId = await _memberIdentityService.GetMemberIdAsync();
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
            var updated = await _service.UpdateAsync(id, entity, memberId);
            if (updated is null)
                return NotFound();

            var result = updated.ToDTO();
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
    public async Task<IActionResult> Delete(int id)
    {
        var memberId = await _memberIdentityService.GetMemberIdAsync();
        try
        {
            await _service.DeleteAsync(id, memberId);
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
