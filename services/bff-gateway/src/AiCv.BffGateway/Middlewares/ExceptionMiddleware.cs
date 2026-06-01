using System.Net;
using System.Text.Json;

namespace AiCv.BffGateway.Middlewares;

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
        try
        {
            await _next(context); // Laisse passer la requête (YARP ou Minimal API)

            // Si YARP n'arrive pas à joindre un service (Service Down)
            if (context.Response.StatusCode == (int)HttpStatusCode.BadGateway || 
                context.Response.StatusCode == (int)HttpStatusCode.ServiceUnavailable)
            {
                await HandleExceptionAsync(context, "Downstream service is unavailable", (int)HttpStatusCode.ServiceUnavailable);
            }
        }
        catch (HttpRequestException ex) // Erreurs de nos appels HttpClient (Agrégations)
        {
            _logger.LogError(ex, "Erreur de communication inter-services.");
            await HandleExceptionAsync(context, "A required internal service is unavailable.", (int)HttpStatusCode.ServiceUnavailable);
        }
        catch (Exception ex) // Crash général
        {
            _logger.LogError(ex, "Erreur inattendue dans le BFF.");
            await HandleExceptionAsync(context, "Internal server error in BFF Gateway.", (int)HttpStatusCode.InternalServerError);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, string message, int statusCode)
    {
        if (!context.Response.HasStarted)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var result = JsonSerializer.Serialize(new
            {
                message = message,
                service = "bff-gateway",
                statusCode = statusCode
            });

            await context.Response.WriteAsync(result);
        }
    }
}