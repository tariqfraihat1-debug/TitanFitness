using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using TitanFitness.Common.Responses;

namespace TitanFitness.Common.Exceptions;

public sealed class GlobalExceptionHandler
    : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "An exception occurred: {Message}",
            exception.Message);

        int statusCode = GetStatusCode(exception);

        ErrorResponse errorResponse = CreateErrorResponse(exception);

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            errorResponse,
            cancellationToken);

        return true;
    }

    private static int GetStatusCode(Exception exception)
    {
        if (exception is ValidationException)
            return StatusCodes.Status400BadRequest;

        if (exception is InvalidOperationException || exception is ArgumentException)
            return StatusCodes.Status409Conflict;

        return StatusCodes.Status500InternalServerError;
    }

    private static ErrorResponse CreateErrorResponse(Exception exception)
    {
        if (exception is ValidationException validationException)
        {
            string[] errors = validationException.Errors
                .Select(error => error.ErrorMessage)
                .ToArray();

            return new ErrorResponse
            {
                Data = null,
                Errors = errors
            };
        }

        if (exception is InvalidOperationException || exception is ArgumentException)
        {
            return new ErrorResponse
            {
                Data = null,
                Errors = new[]
                {
                    exception.Message
                }
            };
        }

        return new ErrorResponse
        {
            Data = null,
            Errors = new[]
            {
                "An unexpected server error occurred."
            }
        };
    }
}