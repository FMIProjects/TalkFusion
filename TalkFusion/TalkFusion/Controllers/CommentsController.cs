using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalkFusion.Data;
using TalkFusion.Models;

namespace TalkFusion.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public CommentsController(
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
            var comment = db.Comments.Find(id);

            if (User.IsInRole("User"))
            {
                var currentUserId = _userManager.GetUserId(User);

                var currentChannel = (from chn in db.Channels.Include("Comments")
                                      where chn.Id == comment.ChannelId
                                      select chn).First();

                var userGroup = (from usrgrp in db.UserGroups
                                 where usrgrp.GroupId == currentChannel.GroupId && usrgrp.UserId == currentUserId
                                 select usrgrp).First();

                if (!(bool)userGroup.IsModerator && comment.UserId != currentUserId)
                {
                    return Redirect("/Groups/Index/");
                }
            }

            if (comment != null)
            {
                db.Comments.Remove(comment);
                db.SaveChanges();
                var currentChannel = (from chn in db.Channels.Include("Comments")
                                      where chn.Id == comment.ChannelId
                                      select chn).First();
                return Redirect("/Groups/Show/" + currentChannel.GroupId + "/" + comment.ChannelId);
            }
            return Redirect("/Groups/Index/");
        }

        public IActionResult Edit(int id)
        {
            var comment = db.Comments.Find(id);
            if (User.IsInRole("User"))
            {
                var currentUserId = _userManager.GetUserId(User);

                if (comment.UserId != currentUserId)
                {
                    return Redirect("/Groups/Index/");
                }
            }
            return View(comment);
        }

        [HttpPost]
        public IActionResult Edit(int id, Comment requestedComment)
        {
            var comment = db.Comments.Find(id);
            if (User.IsInRole("User"))
            {
                var currentUserId = _userManager.GetUserId(User);

                if (comment.UserId != currentUserId)
                {
                    return Redirect("/Groups/Index/");
                }
            }
            requestedComment.ChannelId = comment.ChannelId;
            if (ModelState.IsValid)
            {
                if (comment != null)
                {
                    comment.Text = requestedComment.Text;
                    db.SaveChanges();
                    var currentChannel = (from chn in db.Channels.Include("Comments")
                                          where chn.Id == comment.ChannelId
                                          select chn).First();
                    return Redirect("/Groups/Show/" + currentChannel.GroupId + "/" + comment.ChannelId);
                }
                return Redirect("/Groups/Index/");
            }
            else
            {
                return View(requestedComment);
            }

        }
    }
}
