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
            return View(channel);
        }

        [HttpPost]
        public IActionResult Edit(int id, Channel requestedChannel)
        {
            var channel = db.Channels.Find(id);
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
