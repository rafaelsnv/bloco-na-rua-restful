using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlocoNaRua.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController
    (
        ILogger<UsersController> logger,
        IUserRepository userRepository
    ) : ControllerBase
{
    private readonly ILogger<UsersController> _logger = logger;
    private readonly IUserRepository _userRepository = userRepository;

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userRepository.GetAllAsync();
        return Ok(new List<string> { "User1", "User2" });
    }

}
