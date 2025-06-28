using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using RatingService.Application.DTOs.Requests;
using RatingService.Application.Interfaces;

namespace RatingService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;
        private readonly ILogger<RatingsController> _logger;
        public RatingsController(IRatingService ratingService, ILogger<RatingsController> logger)
        {
            _ratingService = ratingService;
            _logger = logger;
        }

        /// <summary>
        /// Submit a new rating for a service provider.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] SubmitRatingRequest request)
        {
            await _ratingService.SubmitAsync(request);
            return Ok();
        }

        /// <summary>
        /// Get the average rating given to the specified provider.
        /// </summary>
        [HttpGet("average/{providerId}")]
        public async Task<ActionResult<double>> GetAverageRating(Guid providerId)
        {
            try
            {
                var average = await _ratingService.GetAverageRatingAsync(providerId);
                return average;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving average rating for ServiceProvider {ProviderId}.",
                                 providerId);
                return StatusCode(500, "An error occurred while retrieving the average rating.");
            }
        }
    }
}
