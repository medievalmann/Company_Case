using Microsoft.EntityFrameworkCore;
using RatingService.Domain.Entities;
using RatingService.Domain.Interfaces.Repositories;
using RatingService.Infrastructure.Persistence.DbContexts;

namespace RatingService.Infrastructure.Persistence.Repositories
{
    public class RatingRepository : IRatingRepository
    {
        private readonly RatingDbContext _context;

        public RatingRepository(RatingDbContext context)
        {
            _context = context;
        }

        public async Task SaveAsync(Rating rating)
        {
            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();
        }
        public async Task<double> GetAverageRatingByProviderIdAsync(Guid providerId)
        {
            return await _context.Ratings
                .Where(r => r.ProviderId == providerId)
                .AverageAsync(r => (double?)r.Score) ?? 0.0;
        }
    }
}
