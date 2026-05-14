namespace FlashCards.Models
{
    public class Card
    {
        public int Id { get; set; }
        public string Word { get; set; }
        public List<string> Translations { get; set; }
        public List<string> Examples { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
