using RiderHub.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace RiderHub.WebApi.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        HttpStatusCode statusCode;
        string errorType;

        switch (ex)
        {
            case EntityNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                errorType = "EntityNotFound";
                break;

            case BusinessRuleException:
                statusCode = HttpStatusCode.BadRequest;
                errorType = "BusinessRuleViolation";
                break;

            case DuplicateCnhException:
                statusCode = HttpStatusCode.BadRequest;
                errorType = "DuplicateCnh";
                break;
                
            case DuplicateCnpjException:
                statusCode = HttpStatusCode.BadRequest;
                errorType = "DuplicateCnpj";
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                errorType = "ServerError";
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        var result = JsonSerializer.Serialize(new
        {
            error = errorType,
            message = ex.Message
        });

        return context.Response.WriteAsync(result);
    }
}
