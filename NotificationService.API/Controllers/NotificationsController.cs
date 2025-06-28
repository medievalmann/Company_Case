using Common.Events;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Infrastructure.Stores.Interfaces;

namespace NotificationService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationStore _notificationStore;

        public NotificationsController(INotificationStore notificationStore)
        {
            _notificationStore = notificationStore;
        }

        /// <summary>
        /// Endpoint for a Service Provider to get their latest notifications.
        /// Returns and clears notifications so they are not returned again on the next call.
        /// </summary>
        [HttpGet("{providerId}")]
        public ActionResult<IEnumerable<NotificationEvent>> GetNotifications(Guid providerId)
        {
            IEnumerable<NotificationEvent> notifications = _notificationStore.GetNotificationsByUserId(providerId);
            return Ok(notifications);
        }
    }
}
