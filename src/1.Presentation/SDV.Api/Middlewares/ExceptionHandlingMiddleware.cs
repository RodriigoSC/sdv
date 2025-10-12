using SDV.Api.Models;

namespace SDV.Api.Middlewares;

/// <summary>
/// Middleware para capturar e registrar erros globais
/// </summary>
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
            _logger.LogError(ex, "Erro não tratado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var response = new ErrorResponseDto
        {
            Message = "Erro interno do servidor",
            Code = "INTERNAL_SERVER_ERROR",
            Timestamp = DateTime.UtcNow
        };

        // Em desenvolvimento, incluir detalhes do erro
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")))
        {
            response.Message = exception.Message;
        }

        return context.Response.WriteAsJsonAsync(response);
    }
}