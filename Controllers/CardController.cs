using Microsoft.AspNetCore.Mvc;
using FlashCards.Models;

namespace FlashCards.Controllers
{
    public class CardController : Controller
    {
        private readonly ICardRepository _repo;

        public CardController(ICardRepository repo)
        {
            _repo = repo;
        }

        public IActionResult All(int n = 0, string? sort = null)
        {
            IEnumerable<Card> cards = _repo.GetAllCards();

            if (sort == "Word")
                cards = cards.OrderBy(card => card.Word);
            else if (sort == "Category")
                cards = cards.OrderBy(card => 
                    card.Category != null ? card.Category.Title : "");
            else
                cards = cards.OrderBy(card => card.Id);

            if (n > 0)
                cards = cards.Take(n);

            ViewData["Sort"] = sort;
            ViewData["N"] = n;

            return View(cards.ToList());
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
