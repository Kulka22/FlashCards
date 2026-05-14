namespace FlashCards.Models
{
    public interface ICardRepository
    { 
        IEnumerable<Card> GetAllCards();
        bool AddCard(Card obj);
        bool RemoveCard(Card obj);
        bool UpdateCard(Card obj);
 
        IEnumerable<Category> GetCategories();
        bool AddCategory(Category category);
        bool RemoveCategory(Category category);
        bool UpdateCategory(Category category);
    }
}
