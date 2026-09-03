using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.Common.Responses;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Common.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToNoContent(
        this UnitResult<Error> result,
        ControllerBase controller)
    {
        if (result.IsFailure)
            return ToErrorResponse(result.Error, controller);

        return controller.NoContent();
    }

    public static IActionResult ToOk<T>(
        this Result<T, Error> result,
        ControllerBase controller)
    {
        if (result.IsFailure)
            return ToErrorResponse(result.Error, controller);

        return controller.Ok(new
        {
            data = result.Value,
            errors = Array.Empty<string>()
        });
    }

    public static IActionResult ToCreated<T>(
        this Result<T, Error> result,
        ControllerBase controller)
    {
        if (result.IsFailure)
            return ToErrorResponse(result.Error, controller);

        return controller.StatusCode(
            StatusCodes.Status201Created,
            new
            {
                data = result.Value,
                errors = Array.Empty<string>()
            });
    }

    private static IActionResult ToErrorResponse(
        Error error,
        ControllerBase controller)
    {
        ErrorResponse errorResponse = new()
        {
            Data = null,
            Errors = new[]
            {
                error.Message
            }
        };

        return controller.StatusCode(
            (int)error.StatusCode,
            errorResponse);
    }
}