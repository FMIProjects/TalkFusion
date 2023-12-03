using Microsoft.AspNetCore.Mvc;

namespace TalkFusion.Controllers
{
    public class CategoriesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
