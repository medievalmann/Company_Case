using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;

namespace Common.Events
{
    public class NotificationEvent
    {
        /// <summary>
        /// The ID of the user receiving the notification (e.g., provider, customer, admin).
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The message content of the notification.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The type of notification (e.g., RatingGiven, etc.).
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// The timestamp when the notification was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
