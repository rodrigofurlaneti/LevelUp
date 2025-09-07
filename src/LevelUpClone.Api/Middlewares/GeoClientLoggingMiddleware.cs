using LevelUpClone.Domain.Entities;
using LevelUpClone.Domain.Interfaces;

namespace LevelUpClone.Api.Middlewares
{
    public sealed class GeoClientLoggingMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext ctx, IGeoClientLogRepository repo)
        {
            // coleta mínimos do server
            var entry = new GeoClientLogEntry
            {
                RemoteIp = ctx.Connection.RemoteIpAddress?.ToString(),
                ForwardedFor = ctx.Request.Headers["X-Forwarded-For"].ToString(),
                UserAgent = ctx.Request.Headers.UserAgent.ToString(),
                CorrelationId = ctx.TraceIdentifier,
                EnvReferrer = ctx.Request.Headers["Referer"].ToString(),
                EnvPageUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}{ctx.Request.Path}{ctx.Request.QueryString}",
                SessionId = ctx.Request.Cookies[".AspNetCore.Session"]
            };

            // opcional: se o front mandar JSON com dados extras no header (base64/json), você pode parsear aqui

            _ = repo.InsertAsync(entry); // fire-and-forget (ou await se quiser garantir persistência)
            await _next(ctx);
        }
    }
}
