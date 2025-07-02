using BlocoNaRua.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MeetingPresencesController
    (
        ILogger<MeetingPresencesController> logger,
        IMeetingPresencesRepository meetingPresencesRepo
    ) : ControllerBase
{
    private readonly ILogger<MeetingPresencesController> _logger = logger;
    private readonly IMeetingPresencesRepository _meetingPresencesRepo = meetingPresencesRepo;

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetMeetingPresences()
    {
        var presencesList = await _meetingPresencesRepo.GetAllAsync();
        return Ok(new List<string> { "Presence1", "Presence2" });
    }

}
