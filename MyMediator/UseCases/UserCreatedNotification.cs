using MyMediator.Core;

namespace MyMediator.UseCases;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(Guid notificationUserId, string notificationName);
}

public class EmailService : IEmailService
{
    public Task SendWelcomeEmailAsync(Guid notificationUserId, string notificationName)
    {
        return Task.CompletedTask;
    }
}
public class UserCreatedNotification : INotification
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
}

public class EmailNotificationHandler : INotificationHandler<UserCreatedNotification>
{
    private readonly IEmailService _emailService;
    
    public EmailNotificationHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }
    
    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        await _emailService.SendWelcomeEmailAsync(notification.UserId, notification.Name);
    }
}