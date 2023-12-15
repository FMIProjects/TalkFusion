using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TalkFusion.Data;
using TalkFusion.Models;

namespace TalkFusion.Controllers
{
    public class ChannelsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ChannelsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var channel = db.Channels.Find(id);

            if (User.IsInRole("User"))
            {
                var currentUserId = _userManager.GetUserId(User);

                var userGroup = (from usrgrp in db.UserGroups
                                 where usrgrp.GroupId == channel.GroupId && usrgrp.UserId == currentUserId
                                 select usrgrp).First();

                if (!(bool)userGroup.IsModerator)
                {
                    return Redirect("/Groups/Index/");
                }
            }

            if (channel != null)
            {
                db.Channels.Remove(channel);
                db.SaveChanges();

                return Redirect("/Groups/Show/" + channel.GroupId);
            }

            return Redirect("/Groups/Index/");
        }

        public IActionResult Edit(int id)
        {
            var channel = db.Channels.Find(id);
            if (User.IsInRole("User"))
            {
                var currentUserId = _userManager.GetUserId(User);

                var userGroup = (from usrgrp in db.UserGroups
                                 where usrgrp.GroupId == channel.GroupId && usrgrp.UserId == currentUserId
                                 select usrgrp).First();

                // if the current user is not a moderator just yeet him out of the page
                if (!(bool)userGroup.IsModerator)
                {
                    return Redirect("/Groups/Index/");
                }
            }
            return View(channel);
        }

        [HttpPost]
        public IActionResult Edit(int id, Channel requestedChannel)
        {
            var channel = db.Channels.Find(id);
            if (User.IsInRole("User"))
            {
                // select the specific UserGroup to check if the user is a moderator
                var currentUserId = _userManager.GetUserId(User);
                var userGroup = (from usrgrp in db.UserGroups
                                 where usrgrp.GroupId == channel.GroupId && usrgrp.UserId == currentUserId
                                 select usrgrp).First();

                // if the current user is not a moderator just yeet him out of the page
                if (!(bool)userGroup.IsModerator)
                {
                    return Redirect("/Groups/Index/");
                }
            }

            requestedChannel.GroupId = channel.GroupId;

            if (ModelState.IsValid)
            {
                if (channel != null)
                {
                    channel.Title = requestedChannel.Title;
                    channel.Description = requestedChannel.Description;
                    requestedChannel.GroupId = channel.GroupId;
                    db.SaveChanges();
                    return Redirect("/Groups/Show/" + channel.GroupId);

                }
                return Redirect("/Groups/Index/");
            }
            else
            {
                return View(requestedChannel);
            }

        }
	}
}
