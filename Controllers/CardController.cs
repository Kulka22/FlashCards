using Microsoft.AspNetCore.Mvc;
using FlashCards.Models;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;

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

        public IActionResult Details(int id)
        {
            Card? card = _repo.GetAllCards()
                .FirstOrDefault(card => card.Id == id);

            if (card == null)
                return NotFound();

            return View(card);
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

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Card? card = _repo.GetAllCards()
                .FirstOrDefault(card => card.Id == id);

            if (card == null)
                return NotFound();

            ViewBag.Categories = _repo.GetCategories().ToList();

            return View(card);
        }

        [HttpPost]
        public IActionResult Edit(Card card, string translationsText, string examplesText)
        {
            card.Translations = ExtractFromText(translationsText);

            card.Examples = ExtractFromText(examplesText);

            _repo.UpdateCard(card);

            return RedirectToAction("All");
        }

        public IActionResult Remove(int id)
        {
            Card? card = _repo.GetAllCards()
                .FirstOrDefault(card => card.Id == id);

            if (card == null)
                return NotFound();

            _repo.RemoveCard(card);

            return RedirectToAction("All");
        }

        public IActionResult Stat()
        {
            List<Card> cards = _repo.GetAllCards().ToList();

            ViewData["Count"] = cards.Count;

            ViewData["Words"] = cards
                .Select(card => card.Word)
                .Distinct()
                .OrderBy(word => word)
                .ToList();

            ViewData["Translations"] = cards
                .SelectMany(card => card.Translations)
                .Distinct()
                .OrderBy(translation => translation)
                .ToList();

            ViewData["Examples"] = cards
                .SelectMany(card => card.Examples)
                .Distinct()
                .OrderBy(example => example)
                .ToList();

            ViewData["Categories"] = cards
                .Where(card => card.Category != null)
                .Select(card => card.Category!.Title)
                .Distinct()
                .OrderBy(title => title)
                .ToList();

            if (cards.Count > 0)
            {
                ViewData["TranslationsCount"] = new int[]
                {
            cards.Min(card => card.Translations.Count),
            cards.Max(card => card.Translations.Count)
                };

                ViewData["ExamplesCount"] = new int[]
                {
            cards.Min(card => card.Examples.Count),
            cards.Max(card => card.Examples.Count)
                };
            }
            else
            {
                ViewData["TranslationsCount"] = new int[] { 0, 0 };
                ViewData["ExamplesCount"] = new int[] { 0, 0 };
            }

            return View();
        }

        public IActionResult Export(int n = 0, string? sort = null)
        {
            IEnumerable<Card> cards = _repo.GetAllCards();

            if (sort == "Word")
                cards = cards.OrderBy(card => card.Word);
            else if (sort == "Category")
                cards = cards.OrderBy(card => card.Category != null ? card.Category.Title : "");
            else
                cards = cards.OrderBy(card => card.Id);

            if (n > 0)
                cards = cards.Take(n);

            var result = cards.Select(card => new
            {
                card.Id,
                card.Word,
                card.Translations,
                card.Examples,
                Category = card.Category == null ? null : new
                {
                    card.Category.Id,
                    card.Category.Title,
                    card.Category.Description
                }
            }).ToList(); // без ToList выдает при запросе ERR_EMPTY_RESPONSE

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string json = JsonSerializer.Serialize(result, options);

            byte[] bytes = Encoding.UTF8.GetBytes(json);

            return File(bytes, "application/json; charset=utf-8", "cards.json");

            // Либо вот так, чтобы прям в браузере открывался JSON:
            //return Json(result);
            // P.s. передается не Сard напрямую, потому что есть цикл. связь у карточки и категории, поэтому сериализация json может крашнуться
        }

        [HttpGet]
        public IActionResult Test(int n = 0, int categoryId = 0)
        {
            List<Card> cards = _repo.GetAllCards().ToList();

            if (categoryId > 0)
            {
                cards = cards
                    .Where(card => card.CategoryId == categoryId)
                    .ToList();
            }

            if (n > 0)
            {
                cards = cards
                    .Take(n)
                    .ToList();
            }

            ViewBag.N = n;
            ViewBag.CategoryId = categoryId;
            ViewBag.Categories = _repo.GetCategories().ToList();

            return View(cards);
        }

        [HttpPost]
        public IActionResult Test(List<int> cardIds, List<string> answers, int n = 0, int categoryId = 0)
        {
            List<Card> allCards = _repo.GetAllCards().ToList();

            int correctCount = 0;
            int wrongCount = 0;

            List<object> results = new List<object>();

            for (int i = 0; i < cardIds.Count; i++)
            {
                int cardId = cardIds[i];

                Card? card = allCards.FirstOrDefault(card => card.Id == cardId);

                if (card == null)
                    continue;

                string userAnswer = "";

                if (i < answers.Count && answers[i] != null)
                {
                    userAnswer = answers[i].Trim();
                }

                bool isCorrect = card.Translations.Any(translation =>
                    string.Equals(
                        translation.Trim(),
                        userAnswer,
                        StringComparison.OrdinalIgnoreCase));

                if (isCorrect)
                    correctCount++;
                else
                    wrongCount++;

                results.Add(new
                {
                    Card = card,
                    UserAnswer = userAnswer,
                    IsCorrect = isCorrect
                });
            }

            ViewBag.N = n;
            ViewBag.CategoryId = categoryId;
            ViewBag.CorrectCount = correctCount;
            ViewBag.WrongCount = wrongCount;

            return View("TestResult", results);
        }
    }
}
