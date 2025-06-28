using RatingService.Application.DTOs.Requests;

namespace RatingService.Application.Interfaces
{
    public interface IRatingService
    {
        Task SubmitAsync(SubmitRatingRequest request);
        Task<double> GetAverageRatingAsync(Guid providerId);
    }
}
