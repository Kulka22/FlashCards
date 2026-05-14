namespace FlashCards.Models
{
    public class Card
    {
        public int Id { get; set; }
        public string Word { get; set; }
        public List<string> Translations { get; set; } = new();
        public List<string> Examples { get; set; } = new();
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
