using System.Net;
using System.Text.Json;
using FluentValidation;

namespace SmartKasir.Server.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            await HandleUnauthorizedExceptionAsync(context, ex);
        }
        catch (KeyNotFoundException ex)
        {
            await HandleNotFoundExceptionAsync(context, ex);
        }
        catch (InvalidOperationException ex)
        {
            await HandleBadRequestExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleUnexpectedExceptionAsync(context, ex);
        }
    }

    private async Task HandleValidationExceptionAsync(HttpContext context, ValidationException ex)
    {
        _logger.LogWarning("Validation error: {Message}", ex.Message);
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";

        var errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
        var response = new { Type = "ValidationError", Message = "Validation failed", Errors = errors };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private async Task HandleUnauthorizedExceptionAsync(HttpContext context, UnauthorizedAccessException ex)
    {
        _logger.LogWarning("Unauthorized access: {Message}", ex.Message);
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.ContentType = "application/json";
        var response = new { Type = "Unauthorized", Message = ex.Message };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private async Task HandleNotFoundExceptionAsync(HttpContext context, KeyNotFoundException ex)
    {
        _logger.LogWarning("Resource not found: {Message}", ex.Message);
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        context.Response.ContentType = "application/json";
        var response = new { Type = "NotFound", Message = ex.Message };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private async Task HandleBadRequestExceptionAsync(HttpContext context, InvalidOperationException ex)
    {
        _logger.LogWarning("Bad request: {Message}", ex.Message);
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";
        var response = new { Type = "BadRequest", Message = ex.Message };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private async Task HandleUnexpectedExceptionAsync(HttpContext context, Exception ex)
    {
        _logger.LogError(ex, "Unexpected error occurred");
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";
        var response = new { Type = "InternalError", Message = "An unexpected error occurred" };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
