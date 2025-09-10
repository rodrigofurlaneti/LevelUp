using System.Net;
using System.Reflection;
using System.Text.Json;
using LevelUpClone.Domain.Entities;   // GeoClientLogEntry
using LevelUpClone.Domain.Interfaces; // IGeoClientLogRepository
using Microsoft.Extensions.Logging;

namespace LevelUpClone.Api.Middlewares
{
    public sealed class GeoClientLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GeoClientLoggingMiddleware> _logger;

        public GeoClientLoggingMiddleware(RequestDelegate next, ILogger<GeoClientLoggingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext ctx, IGeoClientLogRepository repo)
        {
            try
            {
                var path = ctx.Request.Path.Value ?? string.Empty;

                // Ignora rotas estáticas/técnicas
                if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("/health", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".ico", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    await _next(ctx);
                    return;
                }

                // Método, headers, etc.
                var method = ctx.Request.Method;
                var userAgent = ctx.Request.Headers.UserAgent.ToString();
                var scheme = ctx.Request.Scheme;
                var host = ctx.Request.Host.ToString();
                var queryString = ctx.Request.QueryString.HasValue ? ctx.Request.QueryString.Value ?? string.Empty : string.Empty;
                var referer = ctx.Request.Headers.Referer.ToString() ?? string.Empty;
                var origin = ctx.Request.Headers.Origin.ToString() ?? string.Empty;
                var correlationId = ctx.Request.Headers.TryGetValue("X-Correlation-Id", out var cid) ? cid.ToString() : null;

                // Descobrir IP real (considera proxy)
                var ipAddress = GetClientIp(ctx);

                // Serializa headers (toString para evitar cast)
                var headersDict = ctx.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
                var headersJson = JsonSerializer.Serialize(headersDict);

                // Monta entry com nomes que existem na entidade
                var entry = new GeoClientLogEntry();
                TrySet(entry, "Path", path);
                TrySet(entry, "Method", method);
                TrySet(entry, "UserAgent", userAgent);

                // IMPORTANTE: Use o mesmo nome da coluna/propriedade do seu Entity/DB
                // e o tipo IPAddress para mapear em 'inet'
                TrySet(entry, "RemoteIp", ipAddress);

                TrySet(entry, "CreatedAt", DateTime.UtcNow);
                TrySet(entry, "Scheme", scheme);
                TrySet(entry, "Host", host);
                TrySet(entry, "QueryString", queryString);
                TrySet(entry, "Referer", referer);
                TrySet(entry, "Origin", origin);
                TrySet(entry, "CorrelationId", correlationId);
                TrySet(entry, "HeadersJson", headersJson);

                await repo.InsertAsync(entry); // não deve lançar, mas qualquer coisa cai no catch abaixo
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GeoClientLoggingMiddleware falhou; seguindo sem bloquear a requisição.");
            }

            await _next(ctx);
        }

        private static IPAddress? GetClientIp(HttpContext ctx)
        {
            // Tenta Forwarded/X-Forwarded-For/X-Real-IP (proxy)
            var fwd = ctx.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrWhiteSpace(fwd))
            {
                var first = fwd.Split(',')[0].Trim();
                if (IPAddress.TryParse(first, out var parsed)) return parsed;
            }

            var real = ctx.Request.Headers["X-Real-IP"].ToString();
            if (!string.IsNullOrWhiteSpace(real) && IPAddress.TryParse(real, out var realIp)) return realIp;

            return ctx.Connection.RemoteIpAddress;
        }

        private static void TrySet(object target, string propertyName, object? value)
        {
            if (target is null || string.IsNullOrWhiteSpace(propertyName)) return;

            var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop is null || !prop.CanWrite) return;

            try
            {
                if (value is null)
                {
                    prop.SetValue(target, null);
                    return;
                }

                var destType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                // Se já é atribuível, beleza
                if (destType.IsInstanceOfType(value))
                {
                    prop.SetValue(target, value);
                    return;
                }

                // Suporte explícito a IPAddress (vem como string às vezes)
                if (destType == typeof(IPAddress))
                {
                    if (value is string s && IPAddress.TryParse(s, out var ip))
                        prop.SetValue(target, ip);
                    return;
                }

                // Conversões comuns
                if (destType == typeof(DateTime) && value is DateTime dt)
                {
                    prop.SetValue(target, dt); return;
                }
                if (destType == typeof(DateTimeOffset) && value is DateTimeOffset dto)
                {
                    prop.SetValue(target, dto); return;
                }
                if (destType == typeof(string))
                {
                    prop.SetValue(target, value.ToString()); return;
                }

                // Convert.ChangeType para tipos simples
                var converted = Convert.ChangeType(value, destType);
                prop.SetValue(target, converted);
            }
            catch
            {
                // Não derruba o middleware por falha de conversão
            }
        }
    }
}
