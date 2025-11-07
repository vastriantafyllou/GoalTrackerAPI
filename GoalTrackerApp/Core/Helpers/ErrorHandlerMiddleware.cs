using Serilog;
using System.Net;
using GoalTrackerApp.Exceptions;

namespace GoalTrackerApp.Core.Helpers
{
    public class ErrorHandlerMiddleware
    {
        private readonly ILogger<ErrorHandlerMiddleware> _logger = 
            new LoggerFactory().AddSerilog().CreateLogger<ErrorHandlerMiddleware>();

        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                var logContext = new
                {
                    ExceptionType = exception.GetType().Name,
                    EndPoint = context.Request.Path,
                    Method = context.Request.Method,
                    User = context.User.Identity?.Name ?? "Anonymous",
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    TraceId = context.TraceIdentifier
                };

                _logger.LogError("{ExceptionType} at {Endpoint} {Method} by {User} | Trace={TraceId}",
                    logContext.ExceptionType, logContext.EndPoint, logContext.Method, logContext.User, logContext.TraceId);

                var response = context.Response;
                response.ContentType = "application/json";

                response.StatusCode = exception switch
                {
                    InvalidRegistrationException or
                    InvalidArgumentException or
                    EntityAlreadyExistsException => (int)HttpStatusCode.BadRequest, // 400
                    EntityNotAuthorizedException => (int)HttpStatusCode.Unauthorized,    // 401
                    EntityForbiddenException => (int)HttpStatusCode.Forbidden,          // 403
                    EntityNotFoundException => (int)HttpStatusCode.NotFound,        // 404
                    _ => (int)HttpStatusCode.InternalServerError,                     // 500    
                };

                var result = System.Text.Json.JsonSerializer.Serialize(new { code = response.StatusCode, message = exception?.Message });
                await response.WriteAsync(result);
            }
        }
    }
}