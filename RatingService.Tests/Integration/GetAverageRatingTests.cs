using RatingService.Application.DTOs.Requests;
using System.Net.Http.Json;
using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RatingService.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace RatingService.Tests.Integration
{
    public class GetAverageRatingTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public GetAverageRatingTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAverageRating_ShouldReturnCorrectAverageAndPrintDetails()
        {
            var providerId = Guid.NewGuid();


            var requestList = new List<SubmitRatingRequest>();
            requestList.Add(new SubmitRatingRequest
            {
                ProviderId = providerId,
                CustomerId = Guid.NewGuid(),
                Score = 5,
                Comment = "Great service"
            });
            requestList.Add(new SubmitRatingRequest
            {
                ProviderId = providerId,
                CustomerId = Guid.NewGuid(),
                Score = 4,
                Comment = "Great service"
            });
            requestList.Add(new SubmitRatingRequest
            {
                ProviderId = providerId,
                CustomerId = Guid.NewGuid(),
                Score = 3,
                Comment = "Great service"
            });

            foreach (var request in requestList)
            {
                var res = await _client.PostAsJsonAsync("/api/ratings", request);
                res.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RatingDbContext>();
            var dbRatings = await db.Ratings.Where(r => r.ProviderId == providerId).ToListAsync();


            var response = await _client.GetAsync($"/api/ratings/average/{providerId}");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            double.TryParse(content, out var average).Should().BeTrue();
            average.Should().BeApproximately(4.0, 0.01);
        }

        [Fact]
        public async Task GetAverageRating_WithNoRatings_ReturnsZero()
        {
            var providerId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/ratings/average/{providerId}");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            double.TryParse(content, out var average).Should().BeTrue("response should be a numeric string");

            average.Should().Be(0.0);
        }

        [Fact]
        public async Task GetAverageRating_WhenDatabaseFails_ReturnsInternalServerError()
        {
            var providerId = Guid.Parse("deadbeef-dead-beef-dead-beefdeadbeef");

            try
            {
                await _factory.postgresContainer.StopAsync();
                var response = await _client.GetAsync($"/api/ratings/average/{providerId}");
                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine("Status: " + response.StatusCode);
                Console.WriteLine("Body: " + content);

                response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
                content.Should().Contain("error", "The response body should indicate an error.");
            }
            finally
            {
                await _factory.postgresContainer.StartAsync();
            }
        }
    }
}
