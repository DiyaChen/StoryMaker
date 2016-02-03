using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using My_lab7.Models;

namespace My_lab7.Controllers
{
    public class HomeController : Controller
    {
        private StoryContext db = new StoryContext();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Help()
        {
            ViewBag.Message = "Your application instructions page.";

            return View();
        }

        public ActionResult ExecuteStory()
        {
            if (Session["UserName"] == null)
                return RedirectToAction("../Account/Login");

            return View(db.Collages.ToList());
        }
    }
}