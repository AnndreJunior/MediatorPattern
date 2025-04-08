using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MyMediator.Core;

public static class MediatorExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services, Assembly assembly)
    {
        // Register mediator
        services.AddScoped<IMediator, Mediator>();
        
        // Register handlers
        RegisterHandlers(services, assembly);
        
        return services;
    }
    
    private static void RegisterHandlers(IServiceCollection services, Assembly assembly)
    {
        // Register request handlers
        var requestHandlerTypes = assembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
            .ToList();
            
        foreach (var handlerType in requestHandlerTypes)
        {
            var handlerInterface = handlerType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
                
            services.AddTransient(handlerInterface, handlerType);
        }
        
        // Register notification handlers
        var notificationHandlerTypes = assembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>)))
            .ToList();
            
        foreach (var handlerType in notificationHandlerTypes)
        {
            var handlerInterfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>));
                
            foreach (var handlerInterface in handlerInterfaces)
            {
                services.AddTransient(handlerInterface, handlerType);
            }
        }
    }
}