using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;
using My_lab7.Models;

namespace My_lab7.Controllers
{
    public class CollageController : Controller
    {
        private StoryContext db = new StoryContext();

        // GET: /Collage/
        public ActionResult Index()
        {
            if (Session["UserName"] == null || Session["UserName"].ToString() == "")
                return RedirectToAction("../Account/Login");
            return View(db.Collages.ToList());
        }

        // GET: /Collage/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Collage collage = db.Collages.Find(id);
            if (collage == null) {
                return HttpNotFound();
            }
            return View(collage);
        }

        // GET: /Collage/Create
        public ActionResult Create()
        {
            if (Session["UserName"] == null || Session["UserName"].ToString() == "")
                return RedirectToAction("../Account/Login");
            return View();
        }

        // POST: /Collage/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,name,description,DateOfBuild,username")] Collage collage)
        {
            DateTime time1 = DateTime.Now;
            collage.DateOfBuild = time1.ToString("yyyy-MM-dd");
            string s = Session["UserName"].ToString();
            //Create static collage html page
            DateTime timefile = DateTime.Now;
            string relativeLink = "CollagePages/" + timefile.ToString("yyyyMMddHHmmssfff") + ".html";

            collage.PageLink = relativeLink;

            FileStream fs;

            if (!System.IO.File.Exists(Server.MapPath("~/" + collage.PageLink)))
            {
                fs = new FileStream(Server.MapPath("~/" + collage.PageLink), FileMode.Create, FileAccess.Write);

            }
            else
            {
                fs = new FileStream(Server.MapPath("~/" + collage.PageLink), FileMode.Open, FileAccess.Write);
            }

            StreamWriter sw = new StreamWriter(fs);

            sw.WriteLine("<!DOCTYPE html>");
            sw.WriteLine("<head> <link rel='stylesheet' href='1.css'> <title> Collage: " + collage.name + "</title> </head>");
            sw.WriteLine("<body>");
            sw.WriteLine("<h1 class='center'>Collage: " + collage.name + "</h1>");
            sw.WriteLine("<h2 class='title' > Description: " + collage.description + "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
            sw.WriteLine("Date of Build: " + collage.DateOfBuild + "</h2>");
            sw.WriteLine("<p class='para' > Empty </p>");



            fs.Close();

            collage.UserName = s;
            if (ModelState.IsValid)
            {
                db.Collages.Add(collage);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(collage);
        }

        // GET: /Collage/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Collage collage = db.Collages.Find(id);
            if (collage == null)
            {
                return HttpNotFound();
            }
            return View(collage);
        }

        // POST: /Collage/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,name,description,DateOfBuild,username")] Collage collage)
        {
            Collage temp = db.Collages.Find(new object[] { collage.id });

            if (Session["UserName"] == null)
                return RedirectToAction("../Account/Login");
            else if (!Session["UserName"].ToString().Equals(temp.UserName))
            {
                return RedirectToAction("../Account/Login");
            }


            if (ModelState.IsValid)
            {
                db.Collages.Remove(temp);
                db.Collages.Add(collage);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(collage);
        }

        // GET: /Collage/Delete/5
        [Authorize(Roles = "admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Collage collage = db.Collages.Find(id);
            if (collage == null)
            {
                return HttpNotFound();
            }
            return View(collage);
        }

        // POST: /Collage/Delete/5
        [Authorize(Roles = "admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Collage collage = db.Collages.Find(id);

            IQueryable<Story> storyQuery =
                from str in db.Stories
                where str.collage.id == id
                select str;

            foreach (Story story in storyQuery)
            {
                db.Stories.Remove(story);
            }


            db.Collages.Remove(collage);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
