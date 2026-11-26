using System.Text.Json;
using ProjectManagerApi.Domain.Exceptions;

namespace ProjectManagerApi.API.Middlewares;

/// <summary>
/// Middleware global para capturar excepciones y devolver respuestas JSON estandarizadas.
/// </summary>
public class ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (status, title, detail) = exception switch
        {
            NotFoundException notFound => (
                StatusCodes.Status404NotFound,
                "Recurso no encontrado",
                notFound.Message),
            BusinessRuleException businessRule => (
                StatusCodes.Status409Conflict,
                "Conflicto de regla de negocio",
                businessRule.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Error interno del servidor",
                "Ocurrió un error interno inesperado al procesar la solicitud.")
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Excepción no controlada en {Path}", context.Request.Path);
        }
        else
        {
            _logger.LogWarning("Error controlado ({Status}): {Message}", status, exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;

        var payload = new ErrorResponse(status, title, detail);
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }

    public sealed record ErrorResponse(int Status, string Title, string Detail);
}
