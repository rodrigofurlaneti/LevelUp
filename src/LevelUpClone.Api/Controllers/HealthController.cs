using LevelUpClone.Infrastructure.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace LevelUpClone.Api.Controllers
{
    [ApiController]
    [Route("api/health")]
    public sealed class HealthController : ControllerBase
    {
        private readonly DbHealthChecker _db;
        public HealthController(DbHealthChecker db) => _db = db;

        [HttpGet("database")]
        public async Task<IActionResult> Database(CancellationToken ct)
        {
            var (ok, error) = await _db.CheckAsync(ct);
            if (ok) return Ok(new { status = "UP" });
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                              new { status = "DOWN", error });
        }
    }
}
