using System.Text.Json;
using StaySync.Application.Common.Exceptions;

namespace StaySync.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, errors) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found", (object?)null),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden", (object?)null),
            ValidationException ve => (StatusCodes.Status422UnprocessableEntity, "Validation Error", ve.Errors),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", (object?)null)
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new { title, status = statusCode, errors };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
