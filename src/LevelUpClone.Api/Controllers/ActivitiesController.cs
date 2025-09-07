using LevelUpClone.Api.Contracts.Requests;
using LevelUpClone.Api.Contracts.Responses;
using LevelUpClone.Application.Abstractions;
using LevelUpClone.Application.Cqrs.Activities;
using LevelUpClone.Domain.Enums;
using LevelUpClone.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LevelUpClone.Api.Controllers;

[ApiController]
[Route("api/activities")]
[Authorize]
public sealed class ActivitiesController : ControllerBase
{
    private readonly IDispatcher _dispatcher;
    private readonly IActivityRepository _repo;
    public ActivitiesController(IDispatcher dispatcher, IActivityRepository repo) { _dispatcher = dispatcher; _repo = repo; }

    [HttpGet]
    public ActionResult<IEnumerable<ActivityResponse>> Get([FromQuery] bool onlyActive = true)
    {
        var list = _repo.GetActivities(onlyActive).Select(x => new ActivityResponse
        { ActivityId = x.ActivityId, ActivityName = x.ActivityName, ActivityKind = x.ActivityKind, DefaultPoints = x.DefaultPoints, IsActive = x.IsActive });
        return Ok(list);
    }

    [HttpPost]
    public ActionResult<IdResponse> Create([FromBody] CreateActivityRequest req)
    {
        var kind = Enum.Parse<ActivityKind>(req.ActivityKind);
        var points = kind == ActivityKind.Task ? 2 : -2;
        var id = _dispatcher.Send(new CreateActivityCommand { UserId = req.UserId, ActivityName = req.ActivityName, ActivityKind = (int)kind, DefaultPoints = points });
        return Ok(new IdResponse { Id = id });
    }

    [HttpPut("{activityId}/archive")]
    public IActionResult Archive(int activityId, [FromQuery] bool isActive)
    {
        _repo.SetActivityActive(activityId, isActive);
        return NoContent();
    }
}
