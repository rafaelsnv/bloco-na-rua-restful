using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Models.CarnivalBlock;
using BlocoNaRua.Restful.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarnivalBlocksController
    (
        ILogger<CarnivalBlocksController> logger,
        ICarnivalBlockService carnivalBlockService
    ) : ControllerBase
{
    private readonly ILogger<CarnivalBlocksController> _logger = logger;
    private readonly ICarnivalBlockService _carnivalBlockService = carnivalBlockService;

    [HttpGet("Get")]
    public async Task<IActionResult> GetAllCarnivalBlocks()
    {
        try
        {
            var carnivalBlocksList = await _carnivalBlockService.GetAllBlocksAsync();
            return Ok(carnivalBlocksList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os blocos de carnaval");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpGet("Get/{id}")]
    public async Task<IActionResult> GetCarnivalBlockById(int id)
    {
        try
        {
            var carnivalBlock = await _carnivalBlockService.GetBlockByIdAsync(id);
            if (carnivalBlock == null)
                return NotFound();
            return Ok(carnivalBlock);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar bloco de carnaval com ID: {Id}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpPost("Create")]
    public async Task<IActionResult> CreateCarnivalBlock([FromBody] CarnivalBlockCreate carnivalBlock)
    {
        try
        {
            if (carnivalBlock == null)
                return BadRequest("Dados do bloco são obrigatórios");

            var result = await _carnivalBlockService.CreateBlockAsync(carnivalBlock);

            return CreatedAtAction
            (
                nameof(GetCarnivalBlockById),
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
            _logger.LogError(ex, "Erro ao criar bloco de carnaval");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpPatch("Update/{id}")]
    public async Task<IActionResult> UpdateCarnivalBlock(int id, [FromBody] CarnivalBlockUpdate carnivalBlock)
    {
        try
        {
            if (carnivalBlock == null)
                return BadRequest("Dados de atualização são obrigatórios");

            var success = await _carnivalBlockService.UpdateBlockAsync(id, carnivalBlock, carnivalBlock.MemberId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar bloco de carnaval com ID: {Id}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> DeleteCarnivalBlock(int id)
    {
        try
        {
            var success = await _carnivalBlockService.DeleteBlockAsync(id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar bloco de carnaval com ID: {Id}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }
}
