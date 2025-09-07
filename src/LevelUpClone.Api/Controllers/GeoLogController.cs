using LevelUpClone.Api.Contracts;
using LevelUpClone.Domain.Entities;
using LevelUpClone.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Text.Json;

namespace LevelUpClone.Api.Controllers
{
        [ApiController]
        [Route("api/geolog")]
        public sealed class GeoLogController : ControllerBase
        {
            [HttpPost]
            public async Task<IActionResult> Post([FromBody] GeoClientLogEntry entry, [FromServices] IGeoClientLogRepository repo)
            {
                entry = entry with
                {
                    RemoteIp = entry.RemoteIp ?? HttpContext.Connection.RemoteIpAddress?.ToString(),
                    ForwardedFor = entry.ForwardedFor ?? HttpContext.Request.Headers["X-Forwarded-For"].ToString(),
                    UserAgent = entry.UserAgent ?? HttpContext.Request.Headers.UserAgent.ToString(),
                    CorrelationId = entry.CorrelationId ?? HttpContext.TraceIdentifier
                };
                var id = await repo.InsertAsync(entry);
                return Ok(new { id });
            }
        }
}
