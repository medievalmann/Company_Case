using Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace RatingService.Application.DTOs.Requests
{
    /// <summary>
    /// DTO for submitting a new rating.
    /// </summary>
    public class SubmitRatingRequest
    {
        [NotEmptyGuid]
        public Guid CustomerId { get; set; }

        [NotEmptyGuid]
        public Guid ProviderId { get; set; }

        [Range(1, 5, ErrorMessage = "Rating score must be between 1 and 5.")]
        public int Score { get; set; }

        [MaxLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
        public string? Comment { get; set; }
    }
}
