using System.Net;

namespace TitanFitness.Domain.Common.Errors;

public sealed record Error(HttpStatusCode StatusCode, string Message)
{
    public static Error ValueIsRequired(string fieldName)
    {
        return new Error(HttpStatusCode.BadRequest, $"{fieldName} is required.");
    }

    public static Error InvalidValue(string fieldName, object? fieldValue)
    {
        return new Error(HttpStatusCode.BadRequest, $"{fieldName} has invalid value '{fieldValue}'.");
    }

    public static Error InvalidOperation(string message)
    {
        return new Error(HttpStatusCode.Conflict, message);
    }

    public static Error Validation(string message)
    {
        return new Error(HttpStatusCode.BadRequest, message);
    }

    public static Error EntityNotFound(string entityName, object id)
    {
        return new Error(HttpStatusCode.NotFound, $"{entityName} with ID '{id}' was not found.");
    }

    public static Error EntityAlreadyExists(string entityName, string fieldName, object? fieldValue)
    {
        return new Error(HttpStatusCode.Conflict, $"{entityName} with {fieldName} '{fieldValue}' already exists.");
    }

    public static Error UnauthorizedToAccess(string resourceName)
    {
        return new Error(HttpStatusCode.Unauthorized, $"Unauthorized to access {resourceName}.");
    }

    public static Error ExceedMaxLength(string fieldName, int maxLength)
    {
        return new Error(HttpStatusCode.BadRequest, $"{fieldName} cannot exceed {maxLength} characters.");
    }

    public static Error MinLength(string fieldName, int minLength)
    {
        return new Error(HttpStatusCode.BadRequest, $"{fieldName} must contain at least {minLength} characters.");
    }

    public static Error InvalidFieldFormat(string fieldName, object? fieldValue)
    {
        return new Error(HttpStatusCode.BadRequest, $"{fieldName} has invalid format. Value: '{fieldValue}'.");
    }

    public static Error FieldNotInRange(string fieldName, object? fieldValue, object minimum, object maximum)
    {
        return new Error(
            HttpStatusCode.BadRequest,
            $"{fieldName} value '{fieldValue}' must be between {minimum} and {maximum}.");
    }

    public static Error InternalServerError()
    {
        return new Error(HttpStatusCode.InternalServerError, "An unexpected server error occurred.");
    }
}