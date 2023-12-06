using Microsoft.AspNetCore.Mvc;
using TalkFusion.Data;
using TalkFusion.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TalkFusion.Controllers
{
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext db;

        public GroupsController(ApplicationDbContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            var groups = from g in db.Groups select g;


            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            ViewBag.Groups = groups;

            return View();
        }

        public IActionResult Show(int id)
        {
            var group = db.Groups.Find(id);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            return View(group);
        }

        public IActionResult Edit(int id)
        {

            var group = db.Groups.Find(id);
            group.AllCategories = getAllCategories();

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            return View(group);
        }

        [HttpPost]
        public IActionResult Edit(int id, Group requestedGroup)
        {
            var group = db.Groups.Find(id);
            try
            {
                group.Title = requestedGroup.Title;
                group.Description = requestedGroup.Description;
                group.CategoryId = requestedGroup.CategoryId;
                db.SaveChanges();

                TempData["message"] = "The group named: " + group.Title + " was successfully edited.";

                return RedirectToAction("Index");

            }
            catch (Exception e)
            {
                return View(group);
            }

        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var oldGroup = db.Groups.Find(id);

            TempData["message"] = "The group named: " + oldGroup.Title + " has been succesfully deleted";

            db.Groups.Remove(oldGroup);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult New()
        {

            var dummyGroup = new Group();

            dummyGroup.AllCategories = getAllCategories();

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            return View(dummyGroup);
        }

        [HttpPost]
        public IActionResult New(Group requestedGroup)
        {
            try
            {
                db.Groups.Add(requestedGroup);
                db.SaveChanges();

                TempData["message"] = "The group named: " + requestedGroup.Title + " has been succesfully created";

                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                return View(requestedGroup);
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> getAllCategories()
        {
            var selectList = new List<SelectListItem>();

            var categories = from categ in db.Categories select categ;

            foreach (var category in categories)
            {
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }

            return selectList;
        }

    }
}
