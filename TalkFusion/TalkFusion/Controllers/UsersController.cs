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

            // search engine for users
            var search = "";

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {

                // eeliminate space characters
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                // select all the group Ids taht contain the search string
                List<string> usersIds = db.ApplicationUsers.Where(usr => usr.NickName.Contains(search)).Select(usr => usr.Id).ToList();


                // select the groups whose ids are in the list

                users = from user in db.Users
                        where user.NickName != "Admin" && usersIds.Contains(user.Id)
                        orderby user.UserName
                        select user;

                ViewBag.SearchString = search;
            }



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
            if (User.IsInRole("User"))
            {
                var currentUserId = _userManager.GetUserId(User);
                var userGroup = (from usrgrp in db.UserGroups
                                 where usrgrp.GroupId == id && usrgrp.UserId == currentUserId
                                 select usrgrp).First();

                if (!(bool)userGroup.IsModerator)
                {
                    return Redirect("/Groups/Index");
                }
            }

            var users = (from usrgrp in db.UserGroups.Include(c => c.User)
                         where usrgrp.GroupId == id
                         orderby usrgrp.IsModerator descending
                         select usrgrp);

            ViewBag.CurrentUser = _userManager.GetUserId(User);
            ViewBag.UsersList = users;

            return View();
        }

        [Authorize(Roles = "Admin,User")]
        public IActionResult Promote(string userId, int groupId)
        {
            if (User.IsInRole("User"))
            {   
                var currentUserId = _userManager.GetUserId(User);
                var userGroup = (from usrgrp in db.UserGroups
                                 where usrgrp.GroupId == groupId && usrgrp.UserId == currentUserId
                                 select usrgrp).First();

                if (!(bool)userGroup.IsModerator)
                {
                    return Redirect("/Groups/Index");
                }
            }
            var currentUser = (from usrgrp in db.UserGroups
                               where usrgrp.GroupId == groupId && usrgrp.UserId == userId
                               select usrgrp).First();
            var newModerator = new UserGroup
            {
                UserId = currentUser.UserId,
                GroupId = currentUser.GroupId,
                IsModerator = true
            };
            db.UserGroups.Remove(currentUser);
            db.UserGroups.Add(newModerator);
            db.SaveChanges();
            return Redirect("/Users/GroupUserIndex/" + groupId);
        }

        [Authorize(Roles = "Admin,User")]
        public IActionResult Demote(string userId, int groupId)
        {
            if (User.IsInRole("User"))
            {
                var currentUserId = _userManager.GetUserId(User);
                var userGroup = (from usrgrp in db.UserGroups
                                 where usrgrp.GroupId == groupId && usrgrp.UserId == currentUserId
                                 select usrgrp).First();

                if (!(bool)userGroup.IsModerator)
                {
                    return Redirect("/Groups/Index");
                }
            }
            var currentUser = (from usrgrp in db.UserGroups
                               where usrgrp.GroupId == groupId && usrgrp.UserId == userId
                               select usrgrp).First();
            var newMember = new UserGroup
            {
                UserId = currentUser.UserId,
                GroupId = currentUser.GroupId,
                IsModerator = false
            };
            db.UserGroups.Remove(currentUser);
            db.UserGroups.Add(newMember);
            db.SaveChanges();
            return Redirect("/Users/GroupUserIndex/" + groupId);
        }

        [Authorize(Roles = "Admin,User")]
        public IActionResult Kick(string userId, int groupId)
        {
            if (User.IsInRole("User"))
            {
                var currentUserId = _userManager.GetUserId(User);
                var userGroup = (from usrgrp in db.UserGroups
                                 where usrgrp.GroupId == groupId && usrgrp.UserId == currentUserId
                                 select usrgrp).First();

                if (!(bool)userGroup.IsModerator)
                {
                    return Redirect("/Groups/Index");
                }
            }

            var currentUser = (from usrgrp in db.UserGroups
                               where usrgrp.GroupId == groupId && usrgrp.UserId == userId
                               select usrgrp).First();
            db.UserGroups.Remove(currentUser);
            db.SaveChanges();
            return Redirect("/Users/GroupUserIndex/" + groupId);
        }
    }
}
