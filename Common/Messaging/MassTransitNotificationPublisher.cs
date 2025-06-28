using Common.Events;
using Common.Messaging.Interfaces;
using MassTransit;

namespace Common.Messaging
{
    public class MassTransitNotificationPublisher : INotificationEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public MassTransitNotificationPublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public Task PublishAsync(NotificationEvent notification)
        {
            return _publishEndpoint.Publish(notification);
        }
    }
}
