using System.Net;
using System.Text.Json;
using HealthCare.Common.Exceptions;

namespace HealthCare.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, error) = exception switch
        {
            NotFoundException          => (HttpStatusCode.NotFound,            "Not Found"),
            ConflictException          => (HttpStatusCode.Conflict,            "Conflict"),
            ForbiddenException         => (HttpStatusCode.Forbidden,           "Forbidden"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized,       "Unauthorized"),
            _                          => (HttpStatusCode.InternalServerError, "Internal Server Error"),
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            logger.LogError(exception, "Unhandled exception");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

        var body = JsonSerializer.Serialize(new
        {
            error  = error,
            detail = exception.Message,
        }, JsonOptions);

        await context.Response.WriteAsync(body);
    }
}
