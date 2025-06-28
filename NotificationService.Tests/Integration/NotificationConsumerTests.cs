using Common.Enums;
using Common.Events;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Infrastructure.Stores.Interfaces;

namespace NotificationService.Tests.Integration
{
    public class NotificationConsumerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public NotificationConsumerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task NotificationEvent_WhenConsumed_ShouldBeStoredInMemory()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var testHarness = _factory.Services.GetRequiredService<IBus>();

            var notification = new NotificationEvent
            {
                UserId = userId,
                Message = "You received a test notification.",
                Type = NotificationType.RatingGiven,
                CreatedAt = DateTime.UtcNow
            };

            // Act: Publish notification
            await testHarness.Publish(notification);
            await Task.Delay(1000); // Allow consumer to process

            // Assert: Check if in-memory store received it
            var store = _factory.Services.GetRequiredService<INotificationStore>();
            var result = store.GetNotificationsByUserId(userId).ToList();

            result.Should().NotBeNull();
            result.Should().ContainSingle();
            result[0].Message.Should().Be(notification.Message);
            result[0].Type.Should().Be(notification.Type);
        }
    }
}
