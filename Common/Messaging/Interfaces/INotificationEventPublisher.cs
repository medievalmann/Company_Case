using Common.Events;

namespace Common.Messaging.Interfaces
{
    public interface INotificationEventPublisher
    {
        Task PublishAsync(NotificationEvent notification);
    }
}
