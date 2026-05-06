namespace Domain.FlashCard
{
    public class Card
    {
        public int Id { get; set; }
        public required string Front { get; set; }
        public required string Back { get; set; }
        public string? Context { get; set; }
        public int DeckId { get; set; }
        public Deck? Deck { get; set; }
        public List<CardMetaData>? MetaData { get; set; }
    }
}