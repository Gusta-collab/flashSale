using System.Net;
using System.Text.Json;

namespace FlashSale.Api.Middleware;

/// <summary>
/// Middleware para tratamento global de exceções.
/// Converte exceções em respostas HTTP apropriadas.
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.TraceIdentifier;

        // Log da exceção com CorrelationId
        _logger.LogError(exception, 
            "Erro não tratado. CorrelationId: {CorrelationId}, Path: {Path}", 
            correlationId, context.Request.Path);

        var response = exception switch
        {
            FlashSale.Core.Exceptions.InsufficientStockException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Conflict,
                Message = ex.Message,
                CorrelationId = correlationId
            },
            FlashSale.Core.Exceptions.OrderNotFoundException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Message = ex.Message,
                CorrelationId = correlationId
            },
            FlashSale.Core.Exceptions.DuplicateOrderException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Conflict,
                Message = "Pedido duplicado detectado",
                CorrelationId = correlationId
            },
            FlashSale.Core.Exceptions.ProductNotFoundException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Message = ex.Message,
                CorrelationId = correlationId
            },
            ArgumentException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ex.Message,
                CorrelationId = correlationId
            },
            _ => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "Ocorreu um erro interno. Tente novamente.",
                CorrelationId = correlationId
            }
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.StatusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}

/// <summary>
/// Modelo de resposta de erro padronizado.
/// </summary>
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}
