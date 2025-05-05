namespace MyMediator.Core;

public interface IRequestExecutor<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Execute(TRequest request, CancellationToken cancellationToken);
}
