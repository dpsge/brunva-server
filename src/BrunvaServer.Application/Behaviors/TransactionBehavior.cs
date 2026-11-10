using Mediator;
using BrunvaServer.Application.Abstractions;
using BrunvaServer.Application.Common;

namespace BrunvaServer.Application.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IStatusResult, IResultFactory<TResponse>
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (message is not ITransactionalRequest)
            return await next(message, cancellationToken);

        await unitOfWork.BeginAsync(cancellationToken);
        try
        {
            var response = await next(message, cancellationToken);
            if (response.IsSuccess)
                await unitOfWork.CommitAsync(cancellationToken);
            return response;
        }
        catch (ConflictException exception)
        {
            return TResponse.Failure(HttpStatus.Conflict, exception.Code, exception.Title, exception.Detail);
        }
    }
}
