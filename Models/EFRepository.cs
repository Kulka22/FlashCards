
using Microsoft.EntityFrameworkCore;

namespace FlashCards.Models
{
    public class EFRepository : ICardRepository
    {
        private readonly CardContext context;

        public EFRepository(CardContext context)
        {
            this.context = context;
        }

        public bool AddCard(Card obj)
        {
            bool exists = context.Cards.Any(card => 
                card.Id == obj.Id ||
                (
                    card.Word == obj.Word &&
                    card.CategoryId == obj.CategoryId &&
                    card.Translations.SequenceEqual(obj.Translations) &&
                    card.Examples.SequenceEqual(obj.Examples)
                ));

            if (exists)
              return false;

            context.Cards.Add(obj);
            context.SaveChanges();

            return true;
        }

        public bool AddCategory(Category category)
        {
            bool exists = context.Categories.Any(c =>
                c.Id == category.Id ||
                (
                    c.Title == category.Title &&
                    c.Description == category.Description
                ));

            if (exists)
                return false;

            context.Categories.Add(category);
            context.SaveChanges();

            return true;
        }

        public IEnumerable<Card> GetAllCards()
        {
            return context.Cards.Include(card => card.Category).ToList();
        }

        public IEnumerable<Category> GetCategories()
        {
            return context.Categories.Include(cat => cat.Cards).ToList();
        }

        public bool RemoveCard(Card obj)
        {
            context.Cards.Remove(obj);
            context.SaveChanges();
            return true;
        }

        public bool RemoveCategory(Category category)
        {
            context.Categories.Remove(category);
            context.SaveChanges();
            return true;
        }

        public bool UpdateCard(Card obj)
        {
            context.Cards.Update(obj);
            context.SaveChanges();
            return true;
        }

        public bool UpdateCategory(Category category)
        {
            context.Categories.Update(category);
            context.SaveChanges();
            return true;
        }
    }
}
