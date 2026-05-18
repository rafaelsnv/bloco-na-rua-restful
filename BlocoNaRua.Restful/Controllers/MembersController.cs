using Asp.Versioning;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Mappers;
using BlocoNaRua.Restful.Models.CarnivalBlock;
using BlocoNaRua.Restful.Models.Meeting;
using BlocoNaRua.Restful.Models.Member;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ProducesResponseType(typeof(List<MemberResponse>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public class MembersController(IMembersService service) : ControllerBase
{
    private readonly IMembersService _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(List<MemberResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int? page = null, [FromQuery] int? pageSize = null)
    {
        var list = await _service.GetAllAsync(page, pageSize);
        return Ok(list.Select(MemberMapper.ToDTO).ToList());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null)
            return NotFound();
        var result = MemberMapper.ToDTO(entity);
        return Ok(result);
    }

    [HttpGet("uuid/{uuid}")]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUuid(Guid uuid)
    {
        var entity = await _service.GetByUuidAsync(uuid);
        if (entity is null)
            return NotFound();
        var result = MemberMapper.ToDTO(entity);
        return Ok(result);
    }

    [HttpGet("{id}/blocks")]
    [ProducesResponseType(typeof(List<CarnivalBlockResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMemberCarnivalBlocks(int id)
    {
        var blockMembers = await _service.GetMemberBlocksAsync(id);
        if (blockMembers == null || !blockMembers.Any())
            return NotFound();

        var response = blockMembers.Select(bm => CarnivalBlockMapper.ToDTO(bm.CarnivalBlock)).ToList();
        return Ok(response);
    }

    [HttpGet("{id}/meetings")]
    [ProducesResponseType(typeof(List<MeetingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMemberMeetings(int id)
    {
        var meetings = await _service.GetMemberMeetingsAsync(id);
        if (meetings == null || !meetings.Any())
            return NotFound();

        var response = meetings.Select(MeetingMapper.ToDTO).ToList();
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        var result = MemberMapper.ToDTO(created);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
            var result = MemberMapper.ToDTO(updated);
            return Ok(result);
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

}
