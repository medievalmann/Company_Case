using RatingService.Application.DTOs.Requests;
using System.Net.Http.Json;
using System.Net;
using FluentAssertions;

namespace RatingService.Tests.Integration
{
    public class SubmitRatingValidationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public SubmitRatingValidationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Theory]
        [InlineData(null, "00000000-0000-0000-0000-000000000001", 5)]     // Missing ProviderId
        [InlineData("00000000-0000-0000-0000-000000000002", null, 5)]     // Missing CustomerId
        [InlineData("00000000-0000-0000-0000-000000000002", "00000000-0000-0000-0000-000000000003", 0)] // Invalid Score
        public async Task SubmitRating_WithInvalidRequest_ReturnsBadRequest(string? providerIdStr, string? customerIdStr, int score)
        {
            var request = new SubmitRatingRequest
            {
                ProviderId = providerIdStr != null ? Guid.Parse(providerIdStr) : Guid.Empty,
                CustomerId = customerIdStr != null ? Guid.Parse(customerIdStr) : Guid.Empty,
                Score = score,
                Comment = "Invalid input test"
            };

            var response = await _client.PostAsJsonAsync("/api/ratings", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
