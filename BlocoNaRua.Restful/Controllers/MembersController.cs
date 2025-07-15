using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Models.Member;
using BlocoNaRua.Restful.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController
    (
        ILogger<MembersController> logger,
        IMemberService memberService
    ) : ControllerBase
{
    private readonly ILogger<MembersController> _logger = logger;
    private readonly IMemberService _memberService = memberService;

    [HttpGet("Get")]
    public async Task<IActionResult> GetAllMembers()
    {
        try
        {
            var membersList = await _memberService.GetAllMembersAsync();
            return Ok(membersList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os membros");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpGet("Get/{id}")]
    public async Task<IActionResult> GetMemberById(int id)
    {
        try
        {
            var member = await _memberService.GetMemberByIdAsync(id);
            if (member == null)
                return NotFound();
            return Ok(member);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar membro com ID: {Id}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpPost("Create")]
    public async Task<IActionResult> CreateMember([FromBody] MemberCreate member)
    {
        try
        {
            if (member == null)
                return BadRequest("Dados do membro são obrigatórios");

            var result = await _memberService.CreateMemberAsync(member);
            return CreatedAtAction
            (
                nameof(GetMemberById),
                new { id = result.Id },
                result
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar membro");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpPut("Update/{id}")]
    public async Task<IActionResult> UpdateMember(int id, [FromBody] Member member)
    {
        try
        {
            if (member == null || member.Id != id)
                return BadRequest("Dados de atualização são obrigatórios");

            var success = await _memberService.UpdateMemberAsync(id, member);
            return Accepted();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar membro com ID: {Id}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> DeleteMember(int id)
    {
        try
        {
            var success = await _memberService.DeleteMemberAsync(id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar membro com ID: {Id}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

}
