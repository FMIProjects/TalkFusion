using Microsoft.AspNetCore.Mvc;
using TalkFusion.Data;
using TalkFusion.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace TalkFusion.Controllers
{
    public class CategoriesController : Controller
    {

        private readonly ApplicationDbContext db;

        public CategoriesController (ApplicationDbContext context)
        {
            this.db = context;
        }

        public IActionResult Index()
        {
            var categories = from category in db.Categories
                             select category;

            ViewBag.Categories = categories;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            return View();
        }
    }
}
