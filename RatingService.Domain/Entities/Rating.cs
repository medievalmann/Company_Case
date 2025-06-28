namespace RatingService.Domain.Entities
{
    public class Rating
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid ProviderId { get; private set; }
        public Guid CustomerId { get; private set; }
        public int Score { get; private set; }
        public string Comment { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public Rating(Guid providerId, Guid customerId, int score, string comment)
        {
            ProviderId = providerId;
            CustomerId = customerId;
            Score = score;
            Comment = comment;
        }
        private Rating() { }
    }
}
