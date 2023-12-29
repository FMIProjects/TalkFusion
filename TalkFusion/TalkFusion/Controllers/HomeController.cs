using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TalkFusion.Data;
using TalkFusion.Models;

namespace TalkFusion.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public HomeController(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager, ILogger<HomeController> logger)
        {
            _logger = logger;
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Groups");
            }
            //var popularGroups = db.Groups.Take(3).ToList();
            // the the most 3 popular groups
            var popularGroupsIds = db.UserGroups
                .GroupBy(usrgrp => usrgrp.GroupId)
                .OrderByDescending(group => group.Count())
                .Take(3)
                .Select( grp=> grp.Key)
                .ToList();

            var popularGroups = db.Groups.Where(grp => popularGroupsIds.Contains(grp.Id)).ToList().OrderBy(grp=> grp.Id);
            ViewBag.PopularGroups = popularGroups;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}