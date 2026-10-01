using Microsoft.AspNetCore.Diagnostics;
using ToDo.Application.Exceptions;
using ToDo.Core.Responses;

namespace ToDo.API.ExceptionHandlers;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            httpContext.Response.StatusCode = 499;
            return true;
        }

        var traceId = httpContext.TraceIdentifier;
        var isValidation = exception is TaskValidationException;
        var statusCode = isValidation ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError;
        var message = isValidation ? exception.Message : "Ocorreu um erro inesperado. Tente novamente mais tarde.";

        if (isValidation)
        {
            logger.LogWarning("Validação rejeitada em {Method} {Path}. TraceId: {TraceId}. Motivo: {Reason}", httpContext.Request.Method, httpContext.Request.Path, traceId, message);
        }
        else
        {
            logger.LogError(exception, "Falha inesperada em {Method} {Path}. TraceId: {TraceId}", httpContext.Request.Method, httpContext.Request.Path, traceId);
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.Headers["X-Trace-Id"] = traceId;
        await httpContext.Response.WriteAsJsonAsync(Response<object>.Error(message), cancellationToken);
        return true;
    }
}
