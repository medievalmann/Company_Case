using Microsoft.Extensions.DependencyInjection;
using RatingService.Application.DTOs.Requests;
using RatingService.Infrastructure.Persistence.DbContexts;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Common.Events;
using MassTransit.Testing;

namespace RatingService.Tests.Integration
{
    public class SubmitRatingTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public SubmitRatingTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task SubmitRating_WithValidRequest_ReturnsOk_AndPersistsToDb()
        {
            // Arrange
            var request = new SubmitRatingRequest
            {
                ProviderId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Score = 4,
                Comment = "Great service"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/ratings", request);

            // Assert: HTTP
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Assert: DB
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RatingDbContext>();

            var saved = await db.Ratings.FirstOrDefaultAsync(r =>
                r.ProviderId == request.ProviderId && r.CustomerId == request.CustomerId);

            saved.Should().NotBeNull();
            saved!.Score.Should().Be(request.Score);
            saved.Comment.Should().Be(request.Comment);
        }

        [Fact]
        public async Task SubmitRating_StoresInDatabase_And_PublishesNotificationEvent()
        {
            var request = new SubmitRatingRequest
            {
                ProviderId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Score = 5,
                Comment = "Excellent!"
            };

            var response = await _client.PostAsJsonAsync("/api/ratings", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RatingDbContext>();
            var rating = await db.Ratings.FirstOrDefaultAsync(r =>
                r.ProviderId == request.ProviderId && r.CustomerId == request.CustomerId);

            rating.Should().NotBeNull();
            rating!.Score.Should().Be(request.Score);
            rating.Comment.Should().Be(request.Comment);

            var testHarness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
            var published = testHarness.Published.Select<NotificationEvent>();

            published.Should().ContainSingle(e =>
                e.Context.Message.UserId == request.ProviderId &&
                e.Context.Message.Type == Common.Enums.NotificationType.RatingGiven);
        }
    }
}
