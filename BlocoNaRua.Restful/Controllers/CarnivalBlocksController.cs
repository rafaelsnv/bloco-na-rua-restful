using BlocoNaRua.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarnivalBlocksController
    (
        ILogger<CarnivalBlocksController> logger,
        ICarnivalBlocksRepository carnivalBlocksRepository
    ) : ControllerBase
{
    private readonly ILogger<CarnivalBlocksController> _logger = logger;
    private readonly ICarnivalBlocksRepository _carnivalBlocksRepository = carnivalBlocksRepository;

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetCarnivalBlocks()
    {
        var carnivalBlocks = await _carnivalBlocksRepository.GetAllAsync();
        return Ok(new List<string> { "CarnivalBlock1", "CarnivalBlock2" });
    }

}
