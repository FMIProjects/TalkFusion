using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalkFusion.Data;
using TalkFusion.Models;

namespace TalkFusion.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext db;
        public CommentsController(ApplicationDbContext context)
        {
            db = context;
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var comment = db.Comments.Find(id);
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
            return View(comment);
        }

        [HttpPost]
        public IActionResult Edit(int id, Comment requestedComment)
        {
            var comment = db.Comments.Find(id);
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
