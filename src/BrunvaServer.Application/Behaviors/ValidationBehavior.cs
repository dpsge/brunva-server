using FluentValidation;
using FluentValidation.Results;
using Mediator;
using BrunvaServer.Application.Common;

namespace BrunvaServer.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IStatusResult, IResultFactory<TResponse>
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(message, cancellationToken);

        var context = new ValidationContext<TRequest>(message);
        var failures = new List<ValidationFailure>();
        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);
            failures.AddRange(result.Errors);
        }

        if (failures.Count == 0)
            return await next(message, cancellationToken);

        var errors = failures
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray());

        return TResponse.Validation(errors);
    }
}
