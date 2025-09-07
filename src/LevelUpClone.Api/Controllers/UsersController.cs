using LevelUpClone.Api.Contracts.Requests;
using LevelUpClone.Api.Contracts.Responses;
using LevelUpClone.Application.Abstractions;
using LevelUpClone.Application.Cqrs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LevelUpClone.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IDispatcher _dispatcher;
    public UsersController(IDispatcher dispatcher) => _dispatcher = dispatcher;

    [HttpPost("upsert")]
    public ActionResult<IdResponse> Upsert([FromBody] UpsertUserRequest req)
    {
        var id = _dispatcher.Send(new UpsertUserCommand { UserName = req.UserName, DisplayName = req.DisplayName });
        return Ok(new IdResponse { Id = id });
    }
}
