using Domain.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.FlashCard
{
    public class FlashcardReview
    {
        public Guid UserId { get; set; }
        public required User User { get; set; }
        public int CardId { get; set; }
        public required Card Card { get; set; }
        public DateTime LastReviewedAt { get; set; }
        public int ReviewCount { get; set; }
    }
}
