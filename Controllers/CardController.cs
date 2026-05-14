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

        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Categories = _repo.GetCategories().ToList();

            return View(new Card());
        }

        [HttpPost]
        public IActionResult Add(Card card, string translationsText, string examplesText)
        {
            card.Translations = ExtractFromText(translationsText);

            card.Examples = ExtractFromText(examplesText);

            bool result = _repo.AddCard(card);

            if (!result)
            {
                ViewBag.Categories = _repo.GetCategories().ToList();
                ViewBag.Error = "Такая карточка уже существует.";
                return View(card);
            }

            return RedirectToAction("All");
        }

        private List<string> ExtractFromText(string text)
        {
            return text
                .Split('\n')
                .Select(i => i.Trim())
                .Where(i => i.Length > 0)
                .ToList();
        }
    }
}
