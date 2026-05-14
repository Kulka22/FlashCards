using Microsoft.AspNetCore.Mvc;
using FlashCards.Models;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace FlashCards.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICardRepository _repo;

        public CategoryController(ICardRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult All(int n = 0, string? sort = null)
        {
            IEnumerable<Category> categories = _repo.GetCategories();

            if (sort == "Title")
                categories = categories.OrderBy(category => category.Title);
            else
                categories = categories.OrderBy(category => category.Id);

            if (n > 0)
                categories = categories.Take(n);

            ViewData["Sort"] = sort;
            ViewData["N"] = n;

            return View(categories.ToList());
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View(new Category());
        }

        [HttpPost]
        public IActionResult Add(Category category)
        {
            bool result = _repo.AddCategory(category);

            if (!result)
            {
                ViewBag.Error = "Такая категория уже существует.";
                return View(category);
            }

            return RedirectToAction("All");
        }

        public IActionResult Details(int id)
        {
            Category? category = _repo.GetCategories()
                .FirstOrDefault(category => category.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Category? category = _repo.GetCategories()
                .FirstOrDefault(category => category.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            _repo.UpdateCategory(category);

            return RedirectToAction("All");
        }

        public IActionResult Remove(int id)
        {
            Category? category = _repo.GetCategories()
                .FirstOrDefault(cat => cat.Id == id);

            if (category == null)
                return NotFound();

            if (category.Cards.Count > 0)
            {
                TempData["Error"] = "Нельзя удалить категорию, к которой привязаны карточки.";
                return RedirectToAction("All");
            }

            _repo.RemoveCategory(category);

            return RedirectToAction("All");
        }

        public IActionResult Stat()
        {
            List<Category> categories = _repo.GetCategories().ToList();

            ViewData["Count"] = categories.Count;

            ViewData["Titles"] = categories
                .Select(category => category.Title)
                .Distinct()
                .OrderBy(title => title)
                .ToList();

            ViewData["Descriptions"] = categories
                .Select(category => category.Description)
                .Distinct()
                .OrderBy(description => description)
                .ToList();

            if (categories.Count > 0)
            {
                ViewData["CardsCount"] = new int[]
                {
            categories.Min(category => category.Cards.Count),
            categories.Max(category => category.Cards.Count)
                };
            }
            else
            {
                ViewData["CardsCount"] = new int[] { 0, 0 };
            }

            return View();
        }

        public IActionResult Export(int n = 0, string? sort = null)
        {
            IEnumerable<Category> categories = _repo.GetCategories();

            if (sort == "Title")
                categories = categories.OrderBy(category => category.Title);
            else
                categories = categories.OrderBy(category => category.Id);

            if (n > 0)
                categories = categories.Take(n);

            var result = categories.Select(category => new
            {
                category.Id,
                category.Title,
                category.Description,
                Cards = category.Cards.Select(card => new
                {
                    card.Id,
                    card.Word,
                    card.Translations,
                    card.Examples
                })
            }).ToList();

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string json = JsonSerializer.Serialize(result, options);

            byte[] bytes = Encoding.UTF8.GetBytes(json);

            return File(bytes, "application/json; charset=utf-8", "categories.json");

            // Либо вот так, чтобы прям в браузере открывался JSON:
            //return Json(result); // P.s. тут аналогично Card, по идее нельзя напрямую возвращать Category
        }
    }
}
