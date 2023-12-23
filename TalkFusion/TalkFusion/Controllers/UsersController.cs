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
            // get all users besides the admin
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

            // get all the groups of the user where he is not a moderator
            var joinedGroups = (from grp in db.Groups
                                join usrgrp in db.UserGroups
                                on grp.Id equals usrgrp.GroupId
                                where usrgrp.UserId == user.Id && usrgrp.IsModerator == false
                                select grp);

            // get all the groups that the user moderates
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
            // test if the user is a moderator
            if (User.IsInRole("User"))
            {
                var currentUserId = _userManager.GetUserId(User);
                // get the specific data from the UserGroup table to get the isModerator value
                var userGroup = (from usrgrp in db.UserGroups
                                 where usrgrp.GroupId == id && usrgrp.UserId == currentUserId
                                 select usrgrp).First();

                //if the user is not a moderator just deny access
                if (!(bool)userGroup.IsModerator)
                {
                    return Redirect("/Groups/Index");
                }
            }

            // get all users of the current group , the first in order being the moderators
            var users = (from usrgrp in db.UserGroups.Include(c => c.User)
                         where usrgrp.GroupId == id
                         orderby usrgrp.IsModerator descending
                         select usrgrp);

            // needed in order not to show the Kick/Promote/Demote for the current user
            ViewBag.CurrentUser = _userManager.GetUserId(User);
            ViewBag.UsersList = users;

            return View();
        }

        [Authorize(Roles = "Admin,User")]
        public IActionResult Promote(string userId, int groupId)
        {
            // test for the current user to be a moderator in the current group
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

            // select the userGroup that binds the selected user to the current group
            var currentUser = (from usrgrp in db.UserGroups
                               where usrgrp.GroupId == groupId && usrgrp.UserId == userId
                               select usrgrp).First();

            // make a new data entry
            var newModerator = new UserGroup
            {
                UserId = currentUser.UserId,
                GroupId = currentUser.GroupId,
                IsModerator = true
            };

            //remove the old data entry and add the new one
            db.UserGroups.Remove(currentUser);
            db.UserGroups.Add(newModerator);

            db.SaveChanges();
            return Redirect("/Users/GroupUserIndex/" + groupId);
        }

        [Authorize(Roles = "Admin,User")]
        public IActionResult Demote(string userId, int groupId)
        {
            //same logic as promote
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
            // same logic as promote or demote but no new entry is added in UserGroups
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
