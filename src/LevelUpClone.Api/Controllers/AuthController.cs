using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LevelUpClone.Api.Contracts.Requests;
using LevelUpClone.Api.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace LevelUpClone.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IConfiguration _cfg;
    public AuthController(IConfiguration cfg) => _cfg = cfg;

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest req)
    {
        // Simple demo-only validation. Replace with real user repo lookup if needed.
        if (string.IsNullOrWhiteSpace(req.UserName) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Invalid credentials");

        var jwt = _cfg.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: new[] { new Claim(ClaimTypes.Name, req.UserName) },
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: creds
        );

        return Ok(new LoginResponse { Token = new JwtSecurityTokenHandler().WriteToken(token), DisplayName = req.UserName });
    }
}
