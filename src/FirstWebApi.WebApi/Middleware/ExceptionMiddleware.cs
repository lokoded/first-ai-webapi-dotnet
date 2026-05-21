using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApi.WebApi.Middleware;

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
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        var (statusCode, title, detail) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Não autorizado", exception.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Não encontrado", exception.Message),
            InvalidOperationException => (HttpStatusCode.Conflict, "Conflito", exception.Message),
            ArgumentException => (HttpStatusCode.BadRequest, "Requisição inválida", exception.Message),
            _ => (HttpStatusCode.InternalServerError, "Erro interno", "Ocorreu um erro interno no servidor.")
        };

        context.Response.StatusCode = (int)statusCode;

        var problem = new ProblemDetails
        {
            Type = $"https://httpstatuses.io/{(int)statusCode}",
            Title = title,
            Status = (int)statusCode,
            Detail = detail,
            Instance = context.Request.Path
        };

        var traceId = context.TraceIdentifier;
        if (!string.IsNullOrEmpty(traceId))
            problem.Extensions["traceId"] = traceId;

        var result = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(result);
    }
}
