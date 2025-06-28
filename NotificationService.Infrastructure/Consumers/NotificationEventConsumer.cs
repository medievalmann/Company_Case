using Common.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Infrastructure.Stores.Interfaces;

namespace NotificationService.Infrastructure.Consumers
{
    /// <summary>
    /// Consumes NotificationEvent messages published to RabbitMQ via MassTransit.
    /// Each message represents a new notification for a specific service provider (user).
    /// </summary>
    public class NotificationEventConsumer : IConsumer<NotificationEvent>
    {
        private readonly INotificationStore _notificationStore;
        private readonly ILogger<NotificationEventConsumer> _logger;

        public NotificationEventConsumer(INotificationStore notificationStore, ILogger<NotificationEventConsumer> logger)
        {
            _notificationStore = notificationStore;
            _logger = logger;
        }

        public Task Consume(ConsumeContext<NotificationEvent> context)
        {
            var notification = context.Message;

            _notificationStore.AddNotification(notification);

            _logger.LogInformation("Notification received: ProviderId={ProviderId}, Type={Type}, CreatedAt={CreatedAt}",
                notification.UserId, notification.Type, notification.CreatedAt);

            return Task.CompletedTask;
        }
    }
}
