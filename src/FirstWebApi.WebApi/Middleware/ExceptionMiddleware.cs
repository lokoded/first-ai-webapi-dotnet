using System.Net;
using System.Text.Json;
using FirstWebApi.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApi.WebApi.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro não tratado");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        var (statusCode, title, detail) = exception switch
        {
            ConflictException => (HttpStatusCode.Conflict, "Conflito", exception.Message),
            BadRequestException => (HttpStatusCode.BadRequest, "Requisição inválida", exception.Message),
            UnauthorizedException => (HttpStatusCode.Unauthorized, "Não autorizado", exception.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Não autorizado", "Acesso não autorizado."),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Não encontrado", exception.Message),
            InvalidOperationException => (HttpStatusCode.InternalServerError, "Erro interno", "Ocorreu um erro interno no servidor."),
            ArgumentException => (HttpStatusCode.BadRequest, "Requisição inválida", "A requisição contém dados inválidos."),
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
