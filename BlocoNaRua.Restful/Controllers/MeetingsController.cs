using BlocoNaRua.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MeetingsController
    (
        ILogger<MeetingsController> logger,
        IMeetingsRepository meetingsRepository
    ) : ControllerBase
{
    private readonly ILogger<MeetingsController> _logger = logger;
    private readonly IMeetingsRepository _meetingsRepository = meetingsRepository;

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetMeetings()
    {
        var meetingsList = await _meetingsRepository.GetAllAsync();
        return Ok(new List<string> { "Meeting1", "Meeting2" });
    }

}
