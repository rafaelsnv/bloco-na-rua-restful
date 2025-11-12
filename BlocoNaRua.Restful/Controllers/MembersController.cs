using Asp.Versioning;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Mappers;
using BlocoNaRua.Restful.Models.Member;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class MembersController(IMembersService service) : ControllerBase
{
    private readonly IMembersService _service = service;

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

    [HttpGet("uuid/{uuid}")]
    public async Task<IActionResult> GetByUuid(Guid uuid)
    {
        var entity = await _service.GetByUuidAsync(uuid);
        if (entity is null)
            return NotFound();
        var result = ToDTO(entity);
        return Ok(result);
    }

    [HttpGet("{id}/blocks")]
    public async Task<IActionResult> GetMemberCarnivalBlocks(int id)
    {
        var blockMembers = await _service.GetMemberBlocksAsync(id);
        if (blockMembers == null || !blockMembers.Any())
            return NotFound();

        var response = blockMembers.Select(bm => CarnivalBlockMapper.ToDTO(bm.CarnivalBlock)).ToList();
        return Ok(response);
    }

    [HttpGet("{id}/meetings")]
    public async Task<IActionResult> GetMemberMeetings(int id)
    {
        var meetings = await _service.GetMemberMeetingsAsync(id);
        if (meetings == null || !meetings.Any())
            return NotFound();

        var response = meetings.Select(MeetingMapper.ToDTO).ToList();
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MemberCreate model)
    {
        var entity = new MemberEntity(
            id: 0,
            name: model.Name,
            email: model.Email,
            phone: model.Phone,
            profileImage: model.ProfileImage,
            uuid: new Guid(model.Uuid)
        );
        var created = await _service.CreateAsync(entity);
        var result = ToDTO(created);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] MemberUpdate model, [FromHeader(Name = "X-Logged-Member")] int loggedMember)
    {
        try
        {
            var entity = new MemberEntity(
                id: id,
                name: model.Name,
                email: model.Email,
                phone: model.Phone,
                profileImage: model.ProfileImage,
                uuid: new Guid()
            );
            var updated = await _service.UpdateAsync(id, loggedMember, entity);
            if (updated is null)
                return NotFound();
            var result = ToDTO(updated);
            return Ok(result);
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
            var deleted = await _service.DeleteAsync(id, loggedMember);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    private static MemberDTO ToDTO(MemberEntity entity)
    {
        return new MemberDTO(
            entity.Id,
            entity.Name,
            entity.Email,
            entity.Phone,
            entity.ProfileImage,
            entity.Uuid,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}
