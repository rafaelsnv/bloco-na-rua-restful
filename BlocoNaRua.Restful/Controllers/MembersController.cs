using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController
    (
        ILogger<MembersController> logger,
        IMembersRepository membersRepository
    ) : ControllerBase
{
    private readonly ILogger<MembersController> _logger = logger;
    private readonly IMembersRepository _membersRepository = membersRepository;

    [HttpGet("Get")]
    public async Task<IActionResult> GetAllMembers()
    {
        var membersList = await _membersRepository.GetAllAsync();
        return Ok(membersList);
    }

    [HttpGet("Get/{id}")]
    public async Task<IActionResult> GetMember(int id)
    {
        var member = await _membersRepository.GetByIdAsync(id);
        if (member == null)
            return NotFound();
        return Ok(member);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> CreateMember([FromBody] MemberCreate member)
    {
        if (member == null)
            return BadRequest();

        var entity = new MemberEntity(
            id: 0,
            name: member.Name,
            email: member.Email,
            password: member.Password,
            phone: member.Phone,
            profileImage: member.ProfileImage,
            createdAt: DateTime.UtcNow
        );

        var result = await _membersRepository.AddAsync(entity);
        return CreatedAtAction
        (
            nameof(GetMember),
            new { id = result.Id },
            result
        );
    }

    [HttpPut("Update/{id}")]
    public async Task<IActionResult> UpdateMember(int id, [FromBody] Member member)
    {
        if (member == null || member.Id != id)
            return BadRequest();

        var existingMember = await _membersRepository.GetByIdAsync(id);
        if (existingMember == null)
            return NotFound();

        existingMember.Name = member.Name ?? existingMember.Name;
        existingMember.Email = member.Email ?? existingMember.Email;
        existingMember.Phone = member.Phone ?? existingMember.Phone;
        existingMember.ProfileImage = member.ProfileImage ?? existingMember.ProfileImage;
        existingMember.Password = member.Password ?? existingMember.Password;
        existingMember.UpdatedAt = DateTime.UtcNow;

        await _membersRepository.UpdateAsync(existingMember);
        return Accepted();
    }

    // DELETE: api/Members/{id}
    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> DeleteMember(int id)
    {
        var member = await _membersRepository.GetByIdAsync(id);
        if (member == null)
            return NotFound();

        await _membersRepository.DeleteAsync(member);
        return NoContent();
    }

}
