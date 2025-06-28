using Common.Enums;
using Common.Events;
using Common.Messaging.Interfaces;
using Microsoft.Extensions.Logging;
using RatingService.Application.DTOs.Requests;
using RatingService.Application.Interfaces;
using RatingService.Domain.Entities;
using RatingService.Domain.Interfaces.Repositories;

namespace RatingService.Application.Services
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _repository;
        private readonly INotificationEventPublisher _notificationPublisher;
        private readonly ILogger<RatingService> _logger;

        public RatingService(
            IRatingRepository repository,
            INotificationEventPublisher notificationPublisher,
            ILogger<RatingService> logger)
        {
            _repository = repository;
            _notificationPublisher = notificationPublisher;
            _logger = logger;
        }

        /// <summary>
        /// Submits a new rating from a customer for a provider.
        /// Saves the rating to the database and notifies the provider via the Notification service.
        /// </summary>
        public async Task SubmitAsync(SubmitRatingRequest request)
        {
            var rating = new Rating(
                providerId: request.ProviderId,
                customerId: request.CustomerId,
                score: request.Score,
                comment: request.Comment
            );

            await _repository.SaveAsync(rating);

            try
            {
                var notification = new NotificationEvent
                {
                    UserId = request.ProviderId,
                    Type = NotificationType.RatingGiven,
                    Message = $"You have received a new rating of {rating.Score} stars.",
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationPublisher.PublishAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish notification for new rating of ServiceProvider {ProviderId}.",
                                 rating.ProviderId);
            }


        }
        /// <summary>
        /// Computes the average rating for the given service provider.
        /// Only ratings where ProviderId matches the given provider are considered.
        /// </summary>
        public async Task<double> GetAverageRatingAsync(Guid providerId)
        {
            return await _repository.GetAverageRatingByProviderIdAsync(providerId);
        }
    }
}
