using Microsoft.EntityFrameworkCore;
using RatingService.Domain.Entities;

namespace RatingService.Infrastructure.Persistence.DbContexts
{
    public class RatingDbContext : DbContext
    {
        public RatingDbContext(DbContextOptions<RatingDbContext> options)
            : base(options)
        {
        }
        public DbSet<Rating> Ratings => Set<Rating>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rating>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.ProviderId).IsRequired();
                entity.Property(r => r.CustomerId).IsRequired();
                entity.Property(r => r.Score).IsRequired();
                entity.Property(r => r.Comment).HasMaxLength(500);
                entity.Property(r => r.CreatedAt).IsRequired();
            });
        }
    }
}
