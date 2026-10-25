using System.Net;
using FluentValidation;
using IncidentManagement.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace IncidentManagement.Api.Middleware;

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
        var (statusCode, title, detail) = MapException(exception);

        if (statusCode >= 500)
            _logger.LogError(exception, "Unhandled exception");
        else
            _logger.LogWarning(exception, "Request failed: {Message}", exception.Message);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        if (exception is ValidationException validationException)
        {
            problem.Extensions["errors"] = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        await context.Response.WriteAsJsonAsync(problem);
    }

    private static (int StatusCode, string Title, string Detail) MapException(Exception exception) =>
        exception switch
        {
            ValidationException => ((int)HttpStatusCode.BadRequest, "Validation failed", exception.Message),
            NotFoundException => ((int)HttpStatusCode.NotFound, "Not found", exception.Message),
            ForbiddenException => ((int)HttpStatusCode.Forbidden, "Forbidden", exception.Message),
            UnauthorizedAppException => ((int)HttpStatusCode.Unauthorized, "Unauthorized", exception.Message),
            AppException app => (app.StatusCode, "Bad request", app.Message),
            _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred", "Please contact support if the problem persists.")
        };
}
