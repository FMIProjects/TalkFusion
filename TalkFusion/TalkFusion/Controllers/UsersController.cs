using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalkFusion.Data;
using TalkFusion.Models;

namespace TalkFusion.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }

        // Admin User Panel
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var users = from user in db.Users
                        where user.NickName != "Admin"
                        orderby user.UserName
                        select user;

            ViewBag.UsersList = users;

            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Show(string id)
        {
            ApplicationUser user = db.Users.Find(id);

            var joinedGroups = (from grp in db.Groups
                                join usrgrp in db.UserGroups
                                on grp.Id equals usrgrp.GroupId
                                where usrgrp.UserId == user.Id && usrgrp.IsModerator == false
                                select grp);
            var moderatedGroups = (from grp in db.Groups
                                   join usrgrp in db.UserGroups
                                   on grp.Id equals usrgrp.GroupId
                                   where usrgrp.UserId == user.Id && usrgrp.IsModerator == true
                                   select grp);

            ViewBag.JoinedGroups = joinedGroups;
            ViewBag.ModeratedGroups = moderatedGroups;

            return View(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Delete(string id)
        {
            var user = db.Users
                .Include(u => u.UserGroups)
                    .ThenInclude(ug => ug.Group)
                .Include(u => u.Comments)
                .Where(u => u.Id == id)
                .First();

            // Delete user comments
            if (user.Comments.Count > 0)
            {
                foreach (var comment in user.Comments)
                {
                    db.Comments.Remove(comment);
                }
            }
            
            db.ApplicationUsers.Remove(user);

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // Group UserPanel
        [Authorize(Roles = "Admin,User")]
        public IActionResult GroupUserIndex(int id)
        {
            var users = (from usrgrp in db.UserGroups.Include(c => c.User)
                         where usrgrp.GroupId == id
                         select usrgrp);

            ViewBag.CurrentUser = _userManager.GetUserId(User);
            ViewBag.UsersList = users;

            return View();
        }

    }
}
