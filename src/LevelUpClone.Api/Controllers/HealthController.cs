using Microsoft.AspNetCore.Mvc;

namespace LevelUpClone.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    [HttpGet("status")]
    public IActionResult Status()
    {
        // Mínimo para sair do 404; depois a gente liga no Postgres
        return Ok(new { status = "Up" });
    }

    [HttpGet("database")]
    public IActionResult Database()
    {
        // Mínimo para sair do 404; depois a gente liga no Postgres
        return Ok(new { status = "Up" });
    }
}
