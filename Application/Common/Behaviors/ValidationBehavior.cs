using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using System.Reflection;
using TitanFitness.Domain.Common.Errors;

namespace TitanFitness.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        ValidationContext<TRequest> validationContext = new(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(
                validator => validator.ValidateAsync(
                    validationContext,
                    cancellationToken)));

        var validationFailures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (validationFailures.Count == 0)
            return await next();

        string errorMessage = string.Join(
            " ",
            validationFailures.Select(failure => failure.ErrorMessage));

        Error error = Error.Validation(errorMessage);

        return CreateFailureResult(error);
    }

    private static TResponse CreateFailureResult(Error error)
    {
        Type responseType = typeof(TResponse);

        if (responseType == typeof(UnitResult<Error>))
        {
            UnitResult<Error> result = UnitResult.Failure(error);

            return (TResponse)(object)result;
        }

        if (responseType.IsGenericType &&
            responseType.GetGenericTypeDefinition() == typeof(Result<,>))
        {
            Type[] genericArguments = responseType.GetGenericArguments();

            Type valueType = genericArguments[0];
            Type errorType = genericArguments[1];

            if (errorType != typeof(Error))
                throw new InvalidOperationException($"Unsupported Result error type: {errorType.Name}.");

            MethodInfo failureMethod = typeof(Result)
                .GetMethods()
                .Where(method => method.Name == nameof(Result.Failure))
                .Where(method => method.IsGenericMethodDefinition)
                .Where(method => method.GetGenericArguments().Length == 2)
                .First(method => method.GetParameters().Length == 1);

            MethodInfo genericFailureMethod = failureMethod.MakeGenericMethod(
                valueType,
                typeof(Error));

            object result = genericFailureMethod.Invoke(
                null,
                new object[]
                {
                    error
                })!;

            return (TResponse)result;
        }

        throw new InvalidOperationException($"ValidationBehavior does not support response type {responseType.Name}.");
    }
}