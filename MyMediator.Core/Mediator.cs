using Microsoft.Extensions.DependencyInjection;

namespace MyMediator.Core;

public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    // Send - For command/query operations
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var executorType = typeof(IRequestExecutor<,>).MakeGenericType(requestType, typeof(TResponse));
        var executor = _serviceProvider.GetRequiredService(executorType);

        var method = executorType.GetMethod("Execute");
        var task = (Task<TResponse>)method?.Invoke(executor, [request, cancellationToken])!;
        return await task;
    }

    // Publish - For notification operations
    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notification.GetType());
        var handlers = _serviceProvider.GetServices(handlerType);

        var tasks = new List<Task>();
        foreach (var handler in handlers)
        {
            var method = handlerType.GetMethod("Handle");
            tasks.Add((Task)method.Invoke(handler, new object[] { notification, cancellationToken }));
        }

        await Task.WhenAll(tasks);
    }
}