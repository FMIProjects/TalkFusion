﻿using Microsoft.AspNetCore.Mvc;
using TalkFusion.Data;
using TalkFusion.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;
using Microsoft.AspNetCore.Identity;

namespace TalkFusion.Controllers
{
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public GroupsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
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

        public IActionResult Show(int id,int? channelId)
        {
            var group = (from grp in db.Groups.Include("Category").Include("Channels")
                         where grp.Id == id
                         select grp).First();
            group.Channels = group.Channels.OrderBy(ch => ch.Id).ToList();

            if (channelId != null)
            {
                var currentChannel = (from chn in db.Channels.Include("Comments")
                                      where chn.Id == channelId
                                      select chn).First();
                ViewBag.Channel = currentChannel;
            }
            return View(group);
        }

        // Show  - After New Channel 
        [HttpPost]
        public IActionResult Show([FromForm] Models.Channel? channel)
        {
            // After Creating a Channel
            if (channel != null)
            {
                if (ModelState.IsValid)
                {
                    db.Channels.Add(channel);
                    db.SaveChanges();
                    return Redirect("/Groups/Show/" + channel.GroupId);
                }
                else
                {
                    var group = db.Groups.Include("Category").Include("Channels")
                    .Where(group => group.Id == channel.GroupId)
                    .First();
                    group.Channels = group.Channels.OrderBy(ch => ch.Id).ToList();
                    return View(group);
                }
            }
            return View();
        }

        [HttpPost]
        public IActionResult AddComment([FromForm] Comment comment)
        {
            if (comment != null)
            {
                comment.Date = DateTime.Now;
                if (ModelState.IsValid)
                {
                    db.Comments.Add(comment);
                    db.SaveChanges();
                    ViewBag.Channel = comment.Channel;
                }
                else
                {

                }
            }
            var currentChannel = (from chn in db.Channels.Include("Comments")
                                  where chn.Id == comment.ChannelId
                                  select chn).First();
            return Redirect("/Groups/Show/" + currentChannel.GroupId + "/" + comment.ChannelId);
        }

        public IActionResult Edit(int id)
        {

            var group = (from art in db.Groups.Include("Category")
                         where art.Id == id
                         select art).First();

            group.AllCategories = GetAllCategories();

            return View(group);
        }

        [HttpPost]
        public IActionResult Edit(int id, Group requestedGroup)
        {
            var group = db.Groups.Find(id);
            if (group != null)
                group.AllCategories = GetAllCategories();

            if (ModelState.IsValid)
            {
                if (group != null)
                {
                    group.Title = requestedGroup.Title;
                    group.Description = requestedGroup.Description;
                    group.CategoryId = requestedGroup.CategoryId;
                    db.SaveChanges();

                    TempData["message"] = "The group named: " + group.Title + " was successfully edited.";
                }

                return RedirectToAction("Index");

            }
            else
            {
                return View(group);
            }

        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var oldGroup = db.Groups.Find(id);

            if (oldGroup != null)
            {
                TempData["message"] = "The group named: " + oldGroup.Title + " has been succesfully deleted";

                db.Groups.Remove(oldGroup);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult New()
        {

            var dummyGroup = new Group
            {
                AllCategories = GetAllCategories()
            };

            return View(dummyGroup);
        }

        [HttpPost]
        public IActionResult New(Group requestedGroup)
        {
            requestedGroup.AllCategories = GetAllCategories();
            if (ModelState.IsValid)
            {
              
                db.Groups.Add(requestedGroup);
                db.SaveChanges();

                TempData["message"] = "The group named: " + requestedGroup.Title + " has been succesfully created";

                return RedirectToAction("Index");

            }
            else
            {
                return View(requestedGroup);
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            var selectList = new List<SelectListItem>();

            var categories = from categ in db.Categories select categ;

            foreach (var category in categories)
            {
                if (category != null && category.CategoryName != null)
                {
                    selectList.Add(new SelectListItem
                    {
                        Value = category.Id.ToString(),
                        Text = category.CategoryName.ToString()
                    });
                }

            }

            return selectList;
        }

    }
}