using BlocoNaRua.Data.Repositories.Interfaces;
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

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetMembers()
    {
        var membersList = await _membersRepository.GetAllAsync();
        return Ok(new List<string> { "Member1", "Member2" });
    }

}
