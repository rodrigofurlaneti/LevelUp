using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Text.Json;
using System.Linq; // <= necessário para FirstOrDefault
using LevelUpClone.Api.Contracts.Requests;

namespace LevelUpClone.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public sealed class GeoLogController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        public GeoLogController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("geolog")]
        public IActionResult Post([FromBody] GeoClientLogRequest dto)
        {
            try
            {
                var connStr =
                    _configuration.GetConnectionString("Default")
                    ?? _configuration["Database:Postgres"]
                    ?? _configuration["Database:PostGres"] // se sua chave é "PostGres", mantenha essa linha
                    ?? throw new InvalidOperationException("Connection string not found.");

                const string sql = @"
INSERT INTO ""dbleveluser"".""GeoClientLog""
(
  ""CorrelationId"", ""SessionId"", ""UserAccountId"",
  ""RemoteIp"", ""ForwardedFor"", ""UserAgent"",
  ""Latitude"", ""Longitude"", ""AccuracyMeters"", ""AltitudeMeters"", ""AltitudeAccuracyMeters"",
  ""SpeedMetersPerSecond"", ""HeadingDegrees"", ""TimestampEpochMs"", ""City"",
  ""EnvBrowser"", ""EnvBrowserVersion"", ""EnvOperatingSystem"", ""EnvOSVersion"", ""EnvArchitecture"",
  ""EnvDeviceType"", ""EnvDeviceModel"", ""EnvTouchPoints"", ""EnvIsBot"", ""EnvBotName"",
  ""EnvLanguage"", ""EnvLanguagesJson"", ""EnvPlatform"", ""EnvIsOnline"", ""EnvTimeZone"",
  ""EnvScreenWidth"", ""EnvScreenHeight"", ""EnvDevicePixelRatio"",
  ""EnvReferrer"", ""EnvPageUrl"",
  ""NetDownlink"", ""NetEffectiveType"", ""NetRtt"", ""NetSaveData"",
  ""Error""
)
VALUES
(
  @CorrelationId, @SessionId, NULL,
  CAST(@RemoteIp AS inet), @ForwardedFor, @UserAgent,
  @Latitude, @Longitude, @AccuracyMeters, @AltitudeMeters, @AltitudeAccuracyMeters,
  @SpeedMetersPerSecond, @HeadingDegrees, @TimestampEpochMs, @City,
  @EnvBrowser, @EnvBrowserVersion, @EnvOperatingSystem, @EnvOSVersion, @EnvArchitecture,
  @EnvDeviceType, @EnvDeviceModel, @EnvTouchPoints, @EnvIsBot, @EnvBotName,
  @EnvLanguage, CAST(@EnvLanguagesJson AS jsonb), @EnvPlatform, @EnvIsOnline, @EnvTimeZone,
  @EnvScreenWidth, @EnvScreenHeight, @EnvDevicePixelRatio,
  @EnvReferrer, @EnvPageUrl,
  @NetDownlink, @NetEffectiveType, @NetRtt, @NetSaveData,
  @Error
)
RETURNING ""Id"";";

                // headers/context
                var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                var forwarded = Request.Headers["X-Forwarded-For"].ToString();
                var userAgent = Request.Headers["User-Agent"].ToString();
                var correlationId = Request.Headers["X-Correlation-Id"].FirstOrDefault()
                                  ?? HttpContext.TraceIdentifier; // fallback

                // parâmetros
                var p = new DynamicParameters();
                p.Add("CorrelationId", correlationId);
                p.Add("SessionId", dto.SessionId);

                p.Add("RemoteIp", remoteIp);
                p.Add("ForwardedFor", string.IsNullOrWhiteSpace(forwarded) ? null : forwarded);
                p.Add("UserAgent", string.IsNullOrWhiteSpace(userAgent) ? null : userAgent);

                p.Add("Latitude", dto.Latitude);
                p.Add("Longitude", dto.Longitude);
                p.Add("AccuracyMeters", dto.AccuracyMeters);
                p.Add("AltitudeMeters", dto.AltitudeMeters);
                p.Add("AltitudeAccuracyMeters", dto.AltitudeAccuracyMeters);
                p.Add("SpeedMetersPerSecond", dto.SpeedMetersPerSecond);
                p.Add("HeadingDegrees", dto.HeadingDegrees);
                p.Add("TimestampEpochMs", dto.TimestampEpochMs);
                p.Add("City", dto.City);

                p.Add("EnvBrowser", dto.EnvBrowser);
                p.Add("EnvBrowserVersion", dto.EnvBrowserVersion);
                p.Add("EnvOperatingSystem", dto.EnvOperatingSystem);
                p.Add("EnvOSVersion", dto.EnvOSVersion);
                p.Add("EnvArchitecture", dto.EnvArchitecture);
                p.Add("EnvDeviceType", dto.EnvDeviceType);
                p.Add("EnvDeviceModel", dto.EnvDeviceModel);
                p.Add("EnvTouchPoints", dto.EnvTouchPoints);
                p.Add("EnvIsBot", dto.EnvIsBot);
                p.Add("EnvBotName", dto.EnvBotName);
                p.Add("EnvLanguage", dto.EnvLanguage);

                p.Add("EnvLanguagesJson",
                    dto.EnvLanguagesJson is null ? null : JsonSerializer.Serialize(dto.EnvLanguagesJson, _json));

                p.Add("EnvPlatform", dto.EnvPlatform);
                p.Add("EnvIsOnline", dto.EnvIsOnline);
                p.Add("EnvTimeZone", dto.EnvTimeZone);
                p.Add("EnvScreenWidth", dto.EnvScreenWidth);
                p.Add("EnvScreenHeight", dto.EnvScreenHeight);
                p.Add("EnvDevicePixelRatio", dto.EnvDevicePixelRatio);
                p.Add("EnvReferrer", dto.EnvReferrer);
                p.Add("EnvPageUrl", dto.EnvPageUrl);

                p.Add("NetDownlink", dto.NetDownlink);
                p.Add("NetEffectiveType", dto.NetEffectiveType);
                p.Add("NetRtt", dto.NetRtt);
                p.Add("NetSaveData", dto.NetSaveData);

                p.Add("Error", dto.Error);

                using var conn = new NpgsqlConnection(connStr);
                conn.Open();
                conn.Execute(@"SET search_path TO dbleveluser, public;");
                var newId = conn.ExecuteScalar<int>(sql, p);

                return Created($"/api/geolog/{newId}", new { id = newId, correlationId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
