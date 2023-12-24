using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalkFusion.Data;
using TalkFusion.Models;

namespace TalkFusion.Controllers
{

    [Authorize(Roles="Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public CategoriesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            var categories = from category in db.Categories
                             select category;

            // search engine for categories
            var search = "";

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {

                // eeliminate space characters
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                // select all the group Ids taht contain the search string
                List<int> categoriesIds = db.Categories.Where(categ => categ.CategoryName.Contains(search)).Select(categ => categ.Id).ToList();


                // select the groups whose ids are in the list

                categories = from category in db.Categories
                             where categoriesIds.Contains(category.Id)
                             select category;

                ViewBag.SearchString = search;
            }


            ViewBag.Categories = categories;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            return View();
        }

        public IActionResult Show(int? id)
        {
            var shownCategory = (from category in db.Categories
                                 where category.Id == id
                                 select category)
                                .Include(c => c.Groups)
                                .FirstOrDefault();

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            return View(shownCategory);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = db.Categories.Find(id);
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(int id, Category requestedCategory)
        {
            if (ModelState.IsValid)
            {
                Category? oldCategory = db.Categories.Find(id);

                if (oldCategory != null && requestedCategory != null)
                {
                    oldCategory.CategoryName = requestedCategory.CategoryName;
                    db.SaveChanges();
                    TempData["message"] = "The category named: " + oldCategory.CategoryName + " was successfully edited.";
                }

                return RedirectToAction("Index");
            }
            else
            {
                return View(requestedCategory);
            }
        }

        [HttpGet]
        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        public IActionResult New(Category requestedCategory)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(requestedCategory);
                db.SaveChanges();

                TempData["message"] = "Category named: " + requestedCategory.CategoryName + " has been successfully created.";

                return RedirectToAction("Index");
            }
            else
            {
                return View(requestedCategory);
            }
        }


        [HttpPost]
        public IActionResult Delete(int id)
        {
            Category? oldCategory = db.Categories.Find(id);

            if (oldCategory != null)
            {
                TempData["message"] = "The category named: " + oldCategory.CategoryName + " has been successfully deleted.";
                db.Categories.Remove(oldCategory);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
