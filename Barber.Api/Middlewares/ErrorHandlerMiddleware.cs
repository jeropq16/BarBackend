using System.Net;
using System.Text.Json;

namespace Barber.Api.Middlewares;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Continua el pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log profesional
            _logger.LogError(ex, "Error no manejado en la solicitud");

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        // Código de estado por defecto
        response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // Puedes agregar más tipos personalizados aquí
        switch (ex)
        {
            case ArgumentNullException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;

            case InvalidOperationException:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                break;

            // Caso general → 500
            default:
                break;
        }

        var result = JsonSerializer.Serialize(new
        {
            status = response.StatusCode,
            error = ex.GetType().Name,
            message = ex.Message,
            timestamp = DateTime.UtcNow
        });

        await response.WriteAsync(result);
    }
}