using System.Diagnostics;
using FirstWebApi.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApi.WebApi.Middleware;

public class AppExceptionHandler(ILogger<AppExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellation)
    {
        var (statusCode, title, logLevel) = exception switch
        {
            ConflictException => (StatusCodes.Status409Conflict, "Conflito", LogLevel.Warning),
            BadRequestException => (StatusCodes.Status400BadRequest, "Requisição inválida", LogLevel.Warning),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Não autorizado", LogLevel.Information),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Não autorizado", LogLevel.Warning),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Não encontrado", LogLevel.Warning),
            InvalidOperationException => (StatusCodes.Status500InternalServerError, "Erro interno", LogLevel.Error),
            ArgumentException => (StatusCodes.Status400BadRequest, "Requisição inválida", LogLevel.Warning),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno", LogLevel.Error)
        };

        logger.Log(logLevel, exception, "Erro na requisição {Path}: {Message}", context.Request.Path, exception.Message);

        var problem = new ProblemDetails
        {
            Type = $"https://http.cat/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(problem, cancellation);
        return true;
    }
}
