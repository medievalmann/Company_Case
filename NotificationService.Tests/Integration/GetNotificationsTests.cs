using Common.Enums;
using Common.Events;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Net;

namespace NotificationService.Tests.Integration
{
    public class GetNotificationsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly IBus _bus;

        public GetNotificationsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _bus = factory.Services.GetRequiredService<IBus>();
        }

        [Fact]
        public async Task GetNotifications_ShouldReturnStoredNotification()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var notification = new NotificationEvent
            {
                UserId = userId,
                Message = "Test notification from endpoint test.",
                Type = NotificationType.RatingGiven,
                CreatedAt = DateTime.UtcNow
            };

            // Act: publish the event
            await _bus.Publish(notification);
            await Task.Delay(1000); // wait for consumer to process

            // Make API call
            var response = await _client.GetAsync($"/api/notifications/{userId}");
            var result = await response.Content.ReadFromJsonAsync<List<NotificationEvent>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result.Should().ContainSingle();
            result[0].Message.Should().Be(notification.Message);
        }

        [Fact]
        public async Task GetNotifications_ShouldClearNotificationsAfterFirstFetch()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var notification = new NotificationEvent
            {
                UserId = userId,
                Message = "This is a one-time notification.",
                Type = NotificationType.RatingGiven,
                CreatedAt = DateTime.UtcNow
            };

            // Publish event
            await _bus.Publish(notification);
            await Task.Delay(1000); // Wait for consumer to process

            // First fetch - should return the notification
            var firstResponse = await _client.GetAsync($"/api/notifications/{userId}");
            var firstResult = await firstResponse.Content.ReadFromJsonAsync<List<NotificationEvent>>();

            firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            firstResult.Should().NotBeNull();
            firstResult.Should().ContainSingle();

            // Second fetch - should return empty list
            var secondResponse = await _client.GetAsync($"/api/notifications/{userId}");
            var secondResult = await secondResponse.Content.ReadFromJsonAsync<List<NotificationEvent>>();

            secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            secondResult.Should().NotBeNull();
            secondResult.Should().BeEmpty();
        }
    }

}
