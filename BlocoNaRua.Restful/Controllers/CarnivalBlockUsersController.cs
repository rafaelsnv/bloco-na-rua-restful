using BlocoNaRua.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarnivalBlockUsersController
    (
        ILogger<CarnivalBlockUsersController> logger,
        ICarnivalBlockUsersRepository carnivalBlockUsersRepo
    ) : ControllerBase
{
    private readonly ILogger<CarnivalBlockUsersController> _logger = logger;
    private readonly ICarnivalBlockUsersRepository _carnivalBlockUsersRepo = carnivalBlockUsersRepo;

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetCarnivalBlocks()
    {
        var carnivalBlocks = await _carnivalBlockUsersRepo.GetAllAsync();
        return Ok(new List<string> { "CarnivalBlock1", "CarnivalBlock2" });
    }

}
