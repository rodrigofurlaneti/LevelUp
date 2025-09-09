using System.Reflection;
using System.Text.Json;
using LevelUpClone.Domain.Entities;   // onde está GeoClientLogEntry
using LevelUpClone.Domain.Interfaces; // IGeoClientLogRepository
using Microsoft.Extensions.Logging;

namespace LevelUpClone.Api.Middlewares
{
    /// <summary>
    /// Middleware para registrar informações do request no repositório de GeoClientLog,
    /// sem nunca derrubar a requisição. Usa reflexão para setar apenas as propriedades
    /// que existirem na entidade GeoClientLogEntry.
    /// </summary>
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

                // Monta dados do request
                var method = ctx.Request.Method;
                var userAgent = ctx.Request.Headers.UserAgent.ToString();
                var ip = ctx.Connection.RemoteIpAddress?.ToString();
                var scheme = ctx.Request.Scheme;
                var host = ctx.Request.Host.ToString();
                var queryString = ctx.Request.QueryString.HasValue ? ctx.Request.QueryString.Value : string.Empty;
                var referer = ctx.Request.Headers.Referer.ToString();
                var origin = ctx.Request.Headers.Origin.ToString();
                var correlationId = ctx.Request.Headers.TryGetValue("X-Correlation-Id", out var cid) ? cid.ToString() : null;

                // Serializa headers (útil se sua entidade tiver um campo Raw/Json)
                var headersDict = ctx.Request.Headers.ToDictionary(h => h.Key, h => (string)h.Value);
                var headersJson = JsonSerializer.Serialize(headersDict);

                // Cria a entidade e popula só o que existir
                var entry = new GeoClientLogEntry();
                TrySet(entry, "Path", path);
                TrySet(entry, "Method", method);
                TrySet(entry, "UserAgent", userAgent);
                TrySet(entry, "IpAddress", ip);
                TrySet(entry, "CreatedAt", DateTime.UtcNow);
                TrySet(entry, "Scheme", scheme);
                TrySet(entry, "Host", host);
                TrySet(entry, "QueryString", queryString);
                TrySet(entry, "Referer", referer);
                TrySet(entry, "Origin", origin);
                TrySet(entry, "CorrelationId", correlationId);
                TrySet(entry, "HeadersJson", headersJson);
                // Caso sua entidade tenha um campo genérico (ex.: Payload/Raw/RequestJson):
                // TrySet(entry, "PayloadJson", JsonSerializer.Serialize(new { path, method, userAgent, ip, scheme, host, queryString, referer, origin, correlationId }));

                // Insere sem derrubar a pipeline se falhar
                await repo.InsertAsync(entry);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GeoClientLoggingMiddleware falhou; seguindo sem bloquear a requisição.");
                // nunca rethrow: falha de log não pode quebrar a requisição
            }

            await _next(ctx);
        }

        /// <summary>
        /// Seta uma propriedade por nome se ela existir e for settable.
        /// Converte tipos básicos quando possível (string/DateTime/DateTimeOffset/etc.).
        /// </summary>
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

                if (destType.IsAssignableFrom(value.GetType()))
                {
                    prop.SetValue(target, value);
                    return;
                }

                // Conversões comuns
                if (destType == typeof(DateTime) && value is DateTime dt)
                {
                    prop.SetValue(target, dt);
                    return;
                }
                if (destType == typeof(DateTimeOffset) && value is DateTimeOffset dto)
                {
                    prop.SetValue(target, dto);
                    return;
                }
                if (destType == typeof(string))
                {
                    prop.SetValue(target, value?.ToString());
                    return;
                }

                // Tentativa de Convert.ChangeType para tipos simples
                var converted = Convert.ChangeType(value, destType);
                prop.SetValue(target, converted);
            }
            catch
            {
                // Silencia conversões inválidas: objetivo é nunca quebrar o middleware
            }
        }
    }
}
