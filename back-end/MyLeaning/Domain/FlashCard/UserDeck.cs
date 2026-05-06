using Domain.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.FlashCard
{
    public class UserDeck
    {
        public Guid UserId { get; set; }
        public required User User { get; set; }
        public int DeckId { get; set; }
        public required Deck Deck { get; set; }
        public bool Favorite { get; set; }
    }
}
