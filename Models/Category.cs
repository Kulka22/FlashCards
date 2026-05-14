namespace FlashCards.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<Card> Cards { get; set; }
    }
}
