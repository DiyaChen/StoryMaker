using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml;


using My_lab7.Models;
using System.Data.SqlClient;

namespace My_lab7.Controllers
{
    public class StoryController : Controller
    {
        private StoryContext db = new StoryContext();

        // GET: /Story/
        public ActionResult Index()
        {
            if (Session["UserName"] == null || Session["UserName"].ToString()=="")
                return RedirectToAction("../Account/Login");
            return View(db.Stories.ToList());
        }


        [Authorize(Roles = "admin")]
        public ActionResult ArchiveIndex()
        {
            //if (Session["UserName"] == null || Session["UserName"].ToString() == "")
            //    return RedirectToAction("../Account/Login");
            return View(db.Stories.ToList());
        }


        public ActionResult Archive(int? id)
        {
            if(id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Story story = db.Stories.Find(id);

            if (story.IsArchive != 1)
                 story.IsArchive = 1;
            else
                story.IsArchive = 0;

            db.SaveChanges();
            if (story == null)
            {
                return HttpNotFound();
            }
            return RedirectToAction("../Story/ArchiveIndex");
        }


        // GET: /Story/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Story story = db.Stories.Find(id);
            if (story == null)
            {
                return HttpNotFound();
            }
            return View(story);
        }

     //  [Authorize(Roles = "admin")]
        // GET: /Story/Create
        public ActionResult Create()
        {
            if (Session["UserName"] == null || Session["UserName"].ToString() == "")
                return RedirectToAction("../Account/Login");

            ViewBag.CollageID = new SelectList(db.Collages, "id", "name");
            return View();
        }
        // POST: /Story/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,title,text,DateOfBuild,collage,image,username")] Story story, [Bind(Include = "id, url")] Image image)
        {
            story.image = image;

           string s = Session["UserName"].ToString();

            story.UserName = s;

            HttpPostedFileBase postFile = Request.Files["fileUpload"];
            DateTime time1 = DateTime.Now;

            string uploadPath1 = Server.MapPath("~/CollagePages/UploadedFiles");


            story.DateOfBuild = time1.ToString("yyyy-MM-dd");

            string imgrlt = null;
            if (postFile != null)
            {
                DateTime time = DateTime.Now;

                string uploadPath = Server.MapPath("~/CollagePages/UploadedFiles");

                string fileName = postFile.FileName;

                string filetrype = System.IO.Path.GetExtension(postFile.FileName);

                string savePath = "~/CollagePages/UploadedFiles/";

                string saveFile = uploadPath + fileName + filetrype;
                string savepath = savePath + fileName;

                   // postFile.SaveAs(saveFile);
                    story.image.url = savepath;
                    imgrlt = "./UploadedFiles/" + postFile.FileName;
            }

            if (ModelState.IsValid)
            {
                db.Images.Add(image);
                db.SaveChanges();
            }

            story.collage = db.Collages.Find(new object[] { story.collage.id });






            if (ModelState.IsValid)
            {
                db.Stories.Add(story);
                db.SaveChanges();


                //Assign story to a static html
                DateTime timefile = DateTime.Now;
                string collagePath = Server.MapPath("~/CollagePages/");

                string collageXml = collagePath + story.collage.id.ToString() + ".xml";
                XmlDocument doc = new XmlDocument();

                try
                {
                    if (!System.IO.File.Exists(collageXml))
                    {
                        XmlNode node = doc.CreateXmlDeclaration("1.0", "utf-8", "");
                        doc.AppendChild(node);
                        XmlElement root = doc.CreateElement("Stories");
                        root.SetAttribute("name", story.collage.name);
                        root.SetAttribute("description", story.collage.description);
                        root.SetAttribute("date", story.collage.DateOfBuild);
                        doc.AppendChild(root);
                    }
                    else
                    {
                        doc.Load(collageXml);
                    }


                    XmlElement newStory = doc.CreateElement("Story");
                    newStory.SetAttribute("id",story.id.ToString());
                    XmlElement StoryTitle = doc.CreateElement("Title");
                    StoryTitle.InnerText = story.title;
                    newStory.AppendChild(StoryTitle);
                    XmlElement StoryText = doc.CreateElement("Text");
                    StoryText.InnerText = story.text;
                    newStory.AppendChild(StoryText);
                    XmlElement AbsoluteStoryImage = doc.CreateElement("AbsoluteImage");
                    AbsoluteStoryImage.InnerText = Server.MapPath(story.image.url);
                    newStory.AppendChild(AbsoluteStoryImage);
                    XmlElement RelativeStoryImage = doc.CreateElement("RelativeImage");
                    RelativeStoryImage.InnerText = imgrlt;
                    newStory.AppendChild(RelativeStoryImage);
                    XmlElement StoryDate = doc.CreateElement("DateOfBuild");
                    StoryDate.InnerText = story.DateOfBuild;
                    newStory.AppendChild(StoryDate);
                    doc.DocumentElement.AppendChild(newStory);
                    doc.Save(collageXml);
                }
                catch (Exception ex)
                {
                    Response.Write(ex.Message);
                }


                //Write into html

                WriteIntoCollage("~/"+story.collage.PageLink, collageXml);


                return RedirectToAction("Index");
            }


            return View(story);
        }

        public void WriteIntoCollage(string CollageHtml, string xmlPath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);
            XmlElement rootElem = doc.DocumentElement;
            XmlNodeList listNode = rootElem.GetElementsByTagName("Story");
            XmlNodeList collage = doc.GetElementsByTagName("Stories");

            string CollageUrl = Server.MapPath(CollageHtml);
            FileStream fs;

            if (System.IO.File.Exists(CollageUrl))

                System.IO.File.Delete(CollageUrl);
            
            fs = new FileStream(CollageUrl, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);

            sw.WriteLine("<!DOCTYPE html>");
            sw.WriteLine("<head> <link rel='stylesheet' href='1.css'> <title> Collage: " + rootElem.Attributes["name"] + "</title> </head>");
            sw.WriteLine("<body>");
            sw.WriteLine("<h1 class='center'>Collage: " + collage[0].Attributes["name"].Value + "</h1>");
            sw.WriteLine("<h2 class='title' > Description: " + collage[0].Attributes["description"].Value + "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
            sw.WriteLine("Date of Build: " + collage[0].Attributes["date"].Value + "</h2>");

            foreach (XmlElement StoryNode in listNode)
            {
                XmlNodeList title = StoryNode.GetElementsByTagName("Title");
                XmlNodeList text = StoryNode.GetElementsByTagName("Text");
                XmlNodeList image = StoryNode.GetElementsByTagName("RelativeImage");
                XmlNodeList date = StoryNode.GetElementsByTagName("DateOfBuild");

                sw.WriteLine("<hr> <p class='para' > Title: " + title[0].InnerText + "</p>");
                sw.WriteLine("<p class='para' > <pre>Text: " + text[0].InnerText + "</pre></p>");
                sw.WriteLine("<p class='para' ><img width='240' height='180' src='" + image[0].InnerText + "'/></p>");
                sw.WriteLine("<p class='para' > Date Of Build: " + date[0].InnerText + "</p>");
            }

            sw.WriteLine("</body>");
            sw.WriteLine("</html>");

            sw.Close();
            fs.Close();

        }





        // GET: /Story/Edit/5
        public ActionResult Edit(int? id)
        {


            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Story story = db.Stories.Find(id);
            if (story == null)
            {
                return HttpNotFound();
            }
            ViewBag.CollageID = new SelectList(db.Collages, "id", "name");
            return View(story);
        }

        // POST: /Story/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,title,text,DateOfBuild,collage,image,username")] Story story, [Bind(Include = "id,url")] Image image)
        {
            Story temp = db.Stories.Find(new object[] { story.id });
            story.collage = db.Collages.Find(new object[] { story.collage.id });

            if (Session["UserName"] == null)
                return RedirectToAction("../Account/Login");
            else if (!Session["UserName"].ToString().Equals(temp.UserName))
            {
                return RedirectToAction("../Account/Login");
            }


            string s = Session["UserName"].ToString();

            story.UserName = s;

            story.image = image;
            HttpPostedFileBase postFile = Request.Files["fileUpload"];
            DateTime time1 = DateTime.Now;

            string uploadPath1 = Server.MapPath("~/CollagePages/UploadedFiles");


            story.DateOfBuild = time1.ToString("yyyy-MM-dd");

            string imgrlt = null;
            if (postFile != null)
            {
                DateTime time = DateTime.Now;

                string uploadPath = Server.MapPath("~/CollagePages/UploadedFiles");

                string fileName = postFile.FileName;

                string filetrype = System.IO.Path.GetExtension(postFile.FileName);

                string savePath = "~/CollagePages/UploadedFiles/";

                string saveFile = uploadPath + fileName + filetrype;
                string savepath = savePath + fileName;

                // postFile.SaveAs(saveFile);
                story.image.url = savepath;
                imgrlt = "./UploadedFiles/" + postFile.FileName;
            }


            string collageXml = Server.MapPath("~/CollagePages/") + story.collage.id.ToString() + ".xml";
            try
            {

                XmlDocument myXmlDoc = new XmlDocument();

                myXmlDoc.Load(collageXml);

                XmlElement rootElem = myXmlDoc.DocumentElement;
                XmlNodeList listNode = rootElem.GetElementsByTagName("Story");
                foreach (XmlElement node in listNode)
                {
                    XmlNodeList title = node.GetElementsByTagName("Title");
                    XmlNodeList text = node.GetElementsByTagName("Text");
                    XmlNodeList image1 = node.GetElementsByTagName("RelativeImage");

                    if (node.GetAttribute("id").Equals(story.id.ToString()))
                    {
                        title[0].InnerText = story.title;
                        text[0].InnerText = story.text;
                        image1[0].InnerText = imgrlt;

                    }
                }
                myXmlDoc.Save(collageXml);


                
            }
            catch { }

            WriteIntoCollage("~/" + story.collage.PageLink, collageXml);

            if (ModelState.IsValid)
            {
                db.Stories.Remove(temp);
                db.Stories.Add(story);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(story);
        }

        [Authorize(Roles = "admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Story story = db.Stories.Find(id);
            if (story == null)
            {
                return HttpNotFound();
            }
            return View(story);
        }

        // POST: /Story/Delete/5
        [Authorize(Roles = "admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Story story = db.Stories.Find(id);



            string collageXml = Server.MapPath("~/CollagePages/") + story.collage.PageLink.ToString() + ".xml";
            try
            {

                XmlDocument myXmlDoc = new XmlDocument();

                myXmlDoc.Load(collageXml);

                XmlElement rootElem = myXmlDoc.DocumentElement;
                XmlNodeList listNode = rootElem.GetElementsByTagName("Story");
                foreach (XmlElement node in listNode)
                {
                    XmlNodeList title = node.GetElementsByTagName("Title");
                    XmlNodeList text = node.GetElementsByTagName("Text");
                    XmlNodeList image1 = node.GetElementsByTagName("RelativeImage");

                    if (node.GetAttribute("id").Equals(story.id.ToString()))
                    {

                        rootElem.RemoveChild(node);

                    }
                }
                myXmlDoc.Save(collageXml);



            }
            catch { }
            db.Stories.Remove(story);
            db.SaveChanges();
            WriteIntoCollage("~/" +story.collage.PageLink, collageXml);


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
