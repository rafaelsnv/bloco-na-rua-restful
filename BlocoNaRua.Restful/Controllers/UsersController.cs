using BlocoNaRua.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController
    (
        ILogger<UsersController> logger,
        IUsersRepository userRepository
    ) : ControllerBase
{
    private readonly ILogger<UsersController> _logger = logger;
    private readonly IUsersRepository _userRepository = userRepository;

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetUsers()
    {
        var usersList = await _userRepository.GetAllAsync();
        return Ok(new List<string> { "User1", "User2" });
    }

}
