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
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        
        var handler = _serviceProvider.GetService(handlerType);
        if (handler == null)
            throw new InvalidOperationException($"No handler registered for {requestType.Name}");

        // Get pipeline behaviors
        var behaviors = _serviceProvider.GetServices<IPipelineBehavior<IRequest<TResponse>, TResponse>>();
        
        // Create the request pipeline
        RequestHandlerDelegate<TResponse> pipeline = () =>
        {
            var method = handlerType.GetMethod("Handle");
            return (Task<TResponse>)method.Invoke(handler, new object[] { request, cancellationToken });
        };

        // Apply behaviors in reverse order (so first registered runs first)
        foreach (var behavior in behaviors.Reverse())
        {
            var currentPipeline = pipeline;
            pipeline = () => behavior.Handle(request, currentPipeline, cancellationToken);
        }

        return await pipeline();
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