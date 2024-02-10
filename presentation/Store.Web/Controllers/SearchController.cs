using Microsoft.AspNetCore.Mvc;

namespace Store.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly IBookRepository bookRepositiry;

        public SearchController(IBookRepository bookRepositiry)
        {
            this.bookRepositiry = bookRepositiry;
        }

        public IActionResult Index(string query)
        {
            var books  = bookRepositiry.GetAllByTitle(query);

            return View(books);
        }
    }
}
