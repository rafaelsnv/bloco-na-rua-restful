using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Mappers;
using BlocoNaRua.Restful.Models.CarnivalBlock;
using BlocoNaRua.Services.Interfaces;
using BlocoNaRua.Restful.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Authorize]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(List<CarnivalBlockResponse>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public class CarnivalBlocksController(ICarnivalBlockService service, IMemberIdentityService memberIdentityService) : ControllerBase
{
    private readonly ICarnivalBlockService _service = service;
    private readonly IMemberIdentityService _memberIdentityService = memberIdentityService;

    [HttpGet]
    [ProducesResponseType(typeof(List<CarnivalBlockResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int? page = null, [FromQuery] int? pageSize = null)
    {
        var list = await _service.GetAllAsync(page, pageSize);
        return Ok(list.Select(x => x.ToDTO()));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CarnivalBlockResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null)
            return NotFound();
        var result = entity.ToDTO();
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CarnivalBlockResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        var result = created.ToDTO();
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CarnivalBlockResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(int id, [FromBody] CarnivalBlockUpdate model)
    {
        var memberId = await _memberIdentityService.GetMemberIdAsync();
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
            var updated = await _service.UpdateAsync(id, memberId, entity);
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
