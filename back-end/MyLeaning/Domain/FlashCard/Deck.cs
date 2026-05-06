using Domain.Common;
using Domain.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.FlashCard
{
    public class Deck : BaseEntity
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public Language Language { get; set; }
        public string? Level { get; set; }
        public Guid AuthorId { get; set; }
        public User? Author { get; set; }
        public bool IsPublic { get; set; }
        public List<Card> Cards { get; set; } = new List<Card>();
    }
}
