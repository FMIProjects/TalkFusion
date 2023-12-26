using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
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
            int _perPage = 6;

            // get all users besides the admin
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

            int totalItems = users.Count();

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }
            var paginatedUsers = users.Skip(offset).Take(_perPage);

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            ViewBag.UsersList = paginatedUsers;

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
            var currentUser = db.Users.Find(id);

            if (currentUser == null)
                return RedirectToAction("Index");

            // select all comments of the user
            var comments = (from comm in db.Comments
                            where comm.UserId == id
                            select comm);

            // Delete user comments
            if (comments.Count() > 0)
            {
                foreach (var comment in comments)
                {
                    db.Comments.Remove(comment);
                }
            }

            // get all the user groups
            var UserGroups = (from usrgrp in db.UserGroups
                                       where usrgrp.UserId == id
                                       select usrgrp);
            
            foreach(var userGroup in UserGroups)
            {
                var currentGroup = (from grp in db.Groups
                             where grp.Id == userGroup.GroupId
                             select grp).First();

                var allUsers = (from usrgrp in db.UserGroups
                                where usrgrp.GroupId == currentGroup.Id
                                select usrgrp).Count();

                var allModerators = (from usrgrp in db.UserGroups
                                     where usrgrp.GroupId == currentGroup.Id && usrgrp.IsModerator == true
                                     select usrgrp).Count();

                // delete the group if the user is moderator and he's the only member
                if ( (bool)userGroup.IsModerator && allUsers == 1)
                {
                    db.UserGroups.Remove(userGroup);
                    db.Groups.Remove(currentGroup);
                    //db.SaveChanges();
                }
                else
                {
                    if ((bool)userGroup.IsModerator && allUsers != 1 && allModerators == 1)
                    {
                        // if he's the only moderator make a random member of the group a moderator
                        db.UserGroups.Remove(userGroup);
                        db.SaveChanges();

                        var randomUser = (from usrgrp in db.UserGroups
                                          where usrgrp.GroupId == currentGroup.Id
                                          select usrgrp).First();

                        var newModerator = new UserGroup
                        {
                            UserId = randomUser.UserId,
                            GroupId = randomUser.GroupId,
                            IsModerator = true
                        };
                        db.UserGroups.Remove(randomUser);
                        db.UserGroups.Add(newModerator);
                        //db.SaveChanges();
                    }
                    // if there is another moderator still in the group
                    else
                    {
                        db.UserGroups.Remove(userGroup);
                        //db.SaveChanges();
                    }
                }
            }

            db.ApplicationUsers.Remove(currentUser);

            db.SaveChanges();

            return RedirectToAction("Index");
        }



        // Group UserPanel
        [Authorize(Roles = "Admin,User")]
        public IActionResult GroupUserIndex(int id)
        {
            int _perPage = 6;
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

            // search engine for users in a group
            var search = "";

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {

                // eeliminate space characters
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                // select all the user Ids that contain the search string
                List<string> usersIds = db.ApplicationUsers.Where(usr => usr.NickName.Contains(search)).Select(usr => usr.Id).ToList();


                // select the groups whose ids are in the list

                users = (from usrgrp in db.UserGroups.Include(c => c.User)
                         where usrgrp.GroupId == id && usersIds.Contains(usrgrp.UserId)
                         orderby usrgrp.IsModerator descending
                         select usrgrp);

                ViewBag.SearchString = search;
            }

            int totalItems = users.Count();

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }
            var paginatedUsers = users.Skip(offset).Take(_perPage);

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            // needed in order not to show the Kick/Promote/Demote for the current user
            ViewBag.CurrentUser = _userManager.GetUserId(User);
            ViewBag.UsersList = paginatedUsers;

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

        public IActionResult JoinRequestIndex(int id)
        {
            int _perPage = 6;

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
            var users = (from jrq in db.JoinRequests.Include(c => c.User)
                         where jrq.GroupId==id
                         select jrq);

            // search engine for users requests
            var search = "";

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {

                // eeliminate space characters
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                // select all the user Ids that contain the search string
                List<string> usersIds = db.ApplicationUsers.Where(usr => usr.NickName.Contains(search)).Select(usr => usr.Id).ToList();


                // select the groups whose ids are in the list

                users = (from jrq in db.JoinRequests.Include(c => c.User)
                         where jrq.GroupId == id && usersIds.Contains(jrq.UserId)
                         select jrq);

                ViewBag.SearchString = search;
            }

            int totalItems = users.Count();

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }
            var paginatedUsers = users.Skip(offset).Take(_perPage);

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            if (paginatedUsers.Any())
            {
                ViewBag.UsersList = paginatedUsers;
            }
            return View();
        }

        public IActionResult AcceptRequest(string userId, int groupId)
        {
            var currentJoinRequest = (from jrq in db.JoinRequests
                                      where jrq.GroupId == groupId && jrq.UserId == userId
                                      select jrq).First();

            var newUserGroup = new UserGroup
            {
                UserId = userId,
                GroupId = groupId,
                IsModerator = false
            };

            db.UserGroups.Add(newUserGroup);
            db.JoinRequests.Remove(currentJoinRequest);
            db.SaveChanges();

            return Redirect("/Users/JoinRequestIndex/" + groupId);
        }
    }
}
