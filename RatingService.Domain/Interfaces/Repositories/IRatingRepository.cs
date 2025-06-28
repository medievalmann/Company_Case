using RatingService.Domain.Entities;

namespace RatingService.Domain.Interfaces.Repositories
{
    public interface IRatingRepository
    {
        Task SaveAsync(Rating rating);
        Task<double> GetAverageRatingByProviderIdAsync(Guid providerId);
    }
}
