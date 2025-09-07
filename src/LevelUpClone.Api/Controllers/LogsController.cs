using LevelUpClone.Api.Contracts.Requests;
using LevelUpClone.Api.Contracts.Responses;
using LevelUpClone.Application.Abstractions;
using LevelUpClone.Application.Cqrs.Logs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LevelUpClone.Api.Controllers;

[ApiController]
[Route("api/logs")]
[Authorize]
public sealed class LogsController : ControllerBase
{
    private readonly IDispatcher _dispatcher;
    public LogsController(IDispatcher dispatcher) => _dispatcher = dispatcher;

    [HttpPost("fundamental")]
    public ActionResult<IdResponse> LogFundamental([FromBody] LogFundamentalRequest req)
    {
        var id = _dispatcher.Send(new LogFundamentalCommand { UserId = req.UserId, FundamentalCode = req.FundamentalCode, ActivityDate = req.ActivityDate, NotesText = req.NotesText });
        return Ok(new IdResponse { Id = id });
    }

    [HttpPost("custom")]
    public ActionResult<IdResponse> LogCustom([FromBody] LogCustomRequest req)
    {
        // Points are derived from Activity.DefaultPoints, but for demo we accept +2/-2 via client-side map
        var id = _dispatcher.Send(new LogCustomCommand { UserId = req.UserId, ActivityId = req.ActivityId, ActivityDate = req.ActivityDate, NotesText = req.NotesText, Points = 2 });
        return Ok(new IdResponse { Id = id });
    }
}
