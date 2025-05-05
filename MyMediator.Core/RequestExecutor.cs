
namespace MyMediator.Core;

public class RequestExecutor<TRequest, TResponse>(
    IEnumerable<IPipelineBehavior<TRequest, TResponse>> behaviors,
    IRequestHandler<TRequest, TResponse> handler)
    : IRequestExecutor<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public Task<TResponse> Execute(TRequest request, CancellationToken cancellationToken)
    {
        RequestHandlerDelegate<TResponse> handlerDelegate = () => handler.Handle(request, cancellationToken);

        foreach (var behavior in behaviors.Reverse())
        {
            var current = handlerDelegate;
            handlerDelegate = () => behavior.Handle(request, current, cancellationToken);
        }

        return handlerDelegate();
    }
}
