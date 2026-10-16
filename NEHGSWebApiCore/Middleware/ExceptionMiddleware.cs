using Serilog;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace NEHGSWebApiCore.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                await _next(context);
                sw.Stop();
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Unhandled exception after {ElapsedMs}ms", sw.ElapsedMilliseconds);

                context.Response.ContentType = "application/json";

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = new
                {
                    Success = false,
                    Message = ex.Message
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));

            }
        }
    }
}