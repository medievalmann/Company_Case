using Common.Events;

namespace NotificationService.Infrastructure.Stores.Interfaces
{
    /// <summary>
    /// Interface for a store that holds notification events for service providers in a thread-safe manner.
    /// </summary>
    public interface INotificationStore
    {
        /// <summary>
        /// Adds a new notification event to the store for the event's target user (e.g., service provider).
        /// </summary>
        /// <param name="notificationEvent">The notification event to store.</param>
        void AddNotification(NotificationEvent notificationEvent);

        /// <summary>
        /// Retrieves all pending notifications for the specified user (provider) and removes them from the store.
        /// Subsequent calls will not return the same notifications again.
        /// </summary>
        /// <param name="userId">The ID of the user (service provider) whose notifications to retrieve.</param>
        /// <returns>Collection of notification events for that user. Returns an empty collection if none exist.</returns>
        IEnumerable<NotificationEvent> GetNotificationsByUserId(Guid userId);
    }
}
