using Microsoft.AspNetCore.Mvc;
using FlashCards.Models;

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
    }
}
