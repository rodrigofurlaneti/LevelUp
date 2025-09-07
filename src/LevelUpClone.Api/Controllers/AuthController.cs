using LevelUpClone.Api.Contracts.Requests;
using LevelUpClone.Api.Contracts.Responses;
using LevelUpClone.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public AuthController(IConfiguration configuration, IUserService userService) : base()
        {
            _configuration = configuration;
            _userService = userService;
        }

        [HttpPost("login")]
        public ActionResult<LoginResponse> Login([FromBody] LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.UserName))
                return BadRequest("UserName invalid credentials");
            if (string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Password invalid credentials");
            if (string.IsNullOrWhiteSpace(req.UserName) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Invalid credentials");

            var existUsernameAndPassword = await _userService.ValidateAsync(req.UserName, req.Password);

            if (!existUsernameAndPassword)
                return Unauthorized();

            var jwtSection = _configuration.GetSection("Jwt");
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            var keyValue = jwtSection["Key"];

            if (string.IsNullOrWhiteSpace(issuer) ||
                string.IsNullOrWhiteSpace(audience) ||
                string.IsNullOrWhiteSpace(keyValue))
            {
                return Problem(title: "JWT misconfiguration",
                               detail: "Issuer/Audience/Key are required in configuration.",
                               statusCode: StatusCodes.Status500InternalServerError);
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
            new(ClaimTypes.NameIdentifier, userName), // ajuste para o ID real se tiver
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,   // opcional
                expires: expiresAtUtc,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}