using FluentValidation;
using IntegrationBFF.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IntegrationBFF.Api.Errors;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, code) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed", "validation_error"),
            TransactionNotFoundException => (StatusCodes.Status404NotFound, "Transaction not found", "not_found"),
            PartnerNotVerifiedException => (StatusCodes.Status422UnprocessableEntity, "Partner verification failed", "partner_not_verified"),
            PartnerVerificationUnavailableException => (StatusCodes.Status503ServiceUnavailable, "Partner verification unavailable", "partner_verification_unavailable"),
            DbUpdateException => (StatusCodes.Status409Conflict, "Transaction conflict", "transaction_conflict"),
            TimeoutException => (StatusCodes.Status504GatewayTimeout, "Upstream timeout", "upstream_timeout"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected server error", "server_error")
        };

        if (status >= 500)
        {
            logger.LogError(exception, "Request failed with {ErrorCode}", code);
        }
        else
        {
            logger.LogWarning(exception, "Request rejected with {ErrorCode}", code);
        }

        var errors = exception is ValidationException validationException
            ? validationException.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).Distinct().ToArray())
            : null;

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Type = $"https://httpstatuses.com/{status}",
                Title = title,
                Status = status,
                Detail = status < 500 ? exception.Message : null,
                Extensions =
                {
                    ["code"] = code,
                    ["traceId"] = httpContext.TraceIdentifier,
                    ["errors"] = errors
                }
            },
            Exception = exception
        });
    }
}
