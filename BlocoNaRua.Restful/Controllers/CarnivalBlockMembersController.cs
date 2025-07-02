using BlocoNaRua.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarnivalBlockMembersController
    (
        ILogger<CarnivalBlockMembersController> logger,
        ICarnivalBlockMembersRepository carnivalBlockMembersRepo
    ) : ControllerBase
{
    private readonly ILogger<CarnivalBlockMembersController> _logger = logger;
    private readonly ICarnivalBlockMembersRepository _carnivalBlockMembersRepo = carnivalBlockMembersRepo;

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetBlocksMembers()
    {
        var blocksMembersList = await _carnivalBlockMembersRepo.GetAllAsync();
        return Ok(new List<string> { "CarnivalBlock1", "CarnivalBlock2" });
    }

}
