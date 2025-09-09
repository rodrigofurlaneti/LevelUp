using System.Net;
using System.Text.Json;

namespace LevelUpClone.Api.Middlewares
{
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (Exception ex)
            {
                var status = ex is ArgumentException or InvalidOperationException
                    ? HttpStatusCode.BadRequest
                    : HttpStatusCode.InternalServerError;

                var problem = new
                {
                    type = "about:blank",
                    title = status == HttpStatusCode.BadRequest ? "Bad Request" : "Internal Server Error",
                    status = (int)status,
                    detail = ex.Message,
                    traceId = ctx.TraceIdentifier
                };

                ctx.Response.ContentType = "application/problem+json";
                ctx.Response.StatusCode = (int)status;
                var json = JsonSerializer.Serialize(problem);
                await ctx.Response.WriteAsync(json);
            }
        }
    }
}