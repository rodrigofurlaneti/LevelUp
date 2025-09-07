using LevelUpClone.Api.Contracts.Requests;
using LevelUpClone.Api.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LevelUpClone.Application.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LevelUpClone.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController(IConfiguration configuration, IUserService userService) : ControllerBase
    {
        // Com construtor prim�rio, inicialize os campos assim:
        private readonly IConfiguration _configuration = configuration;
        private readonly IUserService _userService = userService;

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req)
        {
            if (req is null)
                return BadRequest("Body � obrigat�rio.");
            if (string.IsNullOrWhiteSpace(req.UserName))
                return BadRequest("UserName inv�lido.");
            if (string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Password inv�lido.");

            var ok = await _userService.ValidateAsync(req.UserName, req.Password);
            if (!ok)
                return Unauthorized();

            var jwtSection = _configuration.GetSection("Jwt");
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            var keyValue = jwtSection["Key"];

            if (string.IsNullOrWhiteSpace(issuer) ||
                string.IsNullOrWhiteSpace(audience) ||
                string.IsNullOrWhiteSpace(keyValue))
            {
                return Problem(
                    title: "JWT misconfiguration",
                    detail: "Issuer/Audience/Key s�o obrigat�rios em appsettings.",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }

            var expiresAtUtc = DateTime.UtcNow.AddHours(1);

            var tokenStr = CreateJwtToken(
                keyValue: keyValue,
                issuer: issuer,
                audience: audience,
                userName: req.UserName,
                expiresAtUtc: expiresAtUtc
            );

            return Ok(new LoginResponse
            {
                Token = tokenStr,
                DisplayName = req.UserName,
                ExpiresAt = expiresAtUtc
            });
        }

        private static string CreateJwtToken(string keyValue, string issuer, string audience, string userName, DateTime expiresAtUtc)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, userName),
                new(ClaimTypes.NameIdentifier, userName), // troque pelo ID real quando tiver
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAtUtc,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
