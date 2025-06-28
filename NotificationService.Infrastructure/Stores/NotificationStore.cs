using System.Collections.Concurrent;
using Common.Events;
using Microsoft.Extensions.Logging;
using NotificationService.Infrastructure.Stores.Interfaces;

namespace NotificationService.Infrastructure.Stores
{
    /// <summary>
    /// In-memory store for notification events, keyed by user (provider) ID. 
    /// Uses a ConcurrentDictionary of ConcurrentQueue to ensure thread-safe operations.
    /// Note: Data is not persisted and will be lost if the service restarts.
    /// </summary>
    public class NotificationStore : INotificationStore
    {
        private readonly ILogger<NotificationStore> _logger;

        // ConcurrentDictionary to hold a queue of events for each provider (keyed by User ID).
        private readonly ConcurrentDictionary<Guid, ConcurrentQueue<NotificationEvent>> _notifications
            = new ConcurrentDictionary<Guid, ConcurrentQueue<NotificationEvent>>();
        public NotificationStore(ILogger<NotificationStore> logger)
        {
            _logger = logger;
        }

        /// Adds a new notification event to the store for the event's target user (can be provider, customer, or admin).
        public void AddNotification(NotificationEvent notificationEvent)
        {
            var queue = _notifications.GetOrAdd(notificationEvent.UserId, _ => new ConcurrentQueue<NotificationEvent>());
            queue.Enqueue(notificationEvent);

            _logger.LogInformation("Notification added to store for provider {ProviderId} at {Timestamp}.",
                notificationEvent.UserId, notificationEvent.CreatedAt);
        }


        public IEnumerable<NotificationEvent> GetNotificationsByUserId(Guid userId)
        {
            // Try to remove the queue of notifications for the given user. This ensures events are returned only once.
            if (_notifications.TryRemove(userId, out ConcurrentQueue<NotificationEvent>? queue))
            {
                var events = new List<NotificationEvent>();
                while (queue.TryDequeue(out NotificationEvent? notification))
                {
                    events.Add(notification);
                }
                _logger.LogInformation("{Count} notifications returned for provider {userId}.",
               events.Count, userId);
                return events;
            }
            else _logger.LogWarning("No notifications found in store for provider {userId}.", userId);

            return Array.Empty<NotificationEvent>();
        }
    }
}
