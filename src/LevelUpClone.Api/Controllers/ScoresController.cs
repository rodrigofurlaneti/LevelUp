using LevelUpClone.Api.Contracts.Responses;
using LevelUpClone.Application.Abstractions;
using LevelUpClone.Application.Cqrs.Scores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LevelUpClone.Api.Controllers;

[ApiController]
[Route("api/scores")]
[Authorize]
public sealed class ScoresController : ControllerBase
{
    private readonly IDispatcher _dispatcher;
    public ScoresController(IDispatcher dispatcher) => _dispatcher = dispatcher;

    [HttpGet("daily")]
    public ActionResult<DailyScoreResponse> GetDaily([FromQuery] int userId, [FromQuery] string activityDate)
    {
        var dto = _dispatcher.Query(new GetDailyScoreQuery { UserId = userId, ActivityDate = activityDate });
        return Ok(new DailyScoreResponse { TotalPoints = dto.TotalPoints });
    }
}
