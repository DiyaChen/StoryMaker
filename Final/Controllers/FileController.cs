using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using System.Data.Entity;
using System.Net.Http.Headers;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using System.Threading.Tasks;
using My_lab7.Models;

namespace My_lab7.Controllers
{
    public class FileController : ApiController
    {
        //----< GET api/File - get list of available files >---------------
        private StoryContext db = new StoryContext();
        int op = 0;
        public IEnumerable<string> Get()
        {
            // available files
            string startPath = System.Web.HttpContext.Current.Server.MapPath("~\\CollagePages");
            string zipPath = System.Web.HttpContext.Current.Server.MapPath("~\\ZipFiles\\stories.zip");

            if (System.IO.File.Exists(zipPath)) {
                System.IO.File.Delete(zipPath);
            }              
            ZipFile.CreateFromDirectory(startPath, zipPath, CompressionLevel.Fastest, true);
            string path = System.Web.HttpContext.Current.Server.MapPath("~\\ZipFiles");
            string[] files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; ++i) {
                files[i] = Path.GetFileName(files[i]);
            }            
            return files;
        }

        //----< GET api/File?fileName=foobar.txt&open=true >---------------
        //----< attempt to open or close FileStream >----------------------

        public HttpResponseMessage Get(string fileName, string open)
        {
            string sessionId;
            var response = new HttpResponseMessage();
            Models.Session session = new Models.Session();
            CookieHeaderValue cookie = Request.Headers.GetCookies("session-id").FirstOrDefault();
            if (cookie == null) {
                sessionId = session.incrSessionId();
                cookie = new CookieHeaderValue("session-id", sessionId);
                cookie.Expires = DateTimeOffset.Now.AddDays(1);
                cookie.Domain = Request.RequestUri.Host;
                cookie.Path = "/";
            } else {
                sessionId = cookie["session-id"].Value;
            }
            try {
                FileStream fs;
                string path = System.Web.HttpContext.Current.Server.MapPath("~\\ZipFiles");
                if (open == "download") {
                    // attempt to open requested fileName
                    string currentFileSpec = path + "\\" + fileName;
                    fs = new FileStream(currentFileSpec, FileMode.Open);
                    session.saveStream(fs, sessionId);
                } else if (open == "upload") {
                    string path1 = System.Web.HttpContext.Current.Server.MapPath("~\\CollagePages\\UploadedFiles\\");
                    string a = Path.GetFileName(fileName);
                    string currentFileSpec = path1 + "\\" + a;
                    fs = new FileStream(currentFileSpec, FileMode.OpenOrCreate);
                    session.saveStream(fs, sessionId);
                } else {
                    // close FileStream 
                    fs = session.getStream(sessionId);
                    session.removeStream(sessionId);
                    fs.Close();
                    string storyXML = HttpContext.Current.Server.MapPath("~\\CollagePages\\UploadedFiles\\") + "story.xml";
                    if (System.IO.File.Exists(storyXML))
                        addStoryBlock(storyXML);
                }
                response.StatusCode = (HttpStatusCode)200;
            } catch {
                response.StatusCode = (HttpStatusCode)400;
            }
            finally {
                // return cookie to save current sessionId 
                response.Headers.AddCookies(new CookieHeaderValue[] { cookie });
            }
            return response;
        }

        //----< GET api/File?blockSize=2048 - get a block of bytes >-------

        public HttpResponseMessage Get(int blockSize)
        {
            // get FileStream and read block

            Models.Session session = new Models.Session();
            CookieHeaderValue cookie = Request.Headers.GetCookies("session-id").FirstOrDefault();
            string sessionId = cookie["session-id"].Value;
            FileStream down = session.getStream(sessionId);
            byte[] Block = new byte[blockSize];
            int bytesRead = down.Read(Block, 0, blockSize);
            if (bytesRead < blockSize) {
                // compress block
                byte[] returnBlock = new byte[bytesRead];
                for (int i = 0; i < bytesRead; ++i)
                    returnBlock[i] = Block[i];
                Block = returnBlock;
            }
            // make response message containing block and cookie
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Headers.AddCookies(new CookieHeaderValue[] { cookie });
            message.Content = new ByteArrayContent(Block);
            return message;
        }

        // POST api/file
        public HttpResponseMessage Post(int blockSize)
        {
            Task<byte[]> task = Request.Content.ReadAsByteArrayAsync();
            byte[] Block = task.Result;
            Models.Session session = new Models.Session();
            CookieHeaderValue cookie = Request.Headers.GetCookies("session-id").FirstOrDefault();
            string sessionId = cookie["session-id"].Value;
            FileStream up = session.getStream(sessionId);
            up.Write(Block, 0, Block.Count());
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Headers.AddCookies(new CookieHeaderValue[] { cookie });
            return message;
        }

        // PUT api/file/5
        public void Put(int id, [FromBody]string value)
        {
            string debug = "debug";
        }

        // DELETE api/file/5
        public void Delete(int id)
        {
            string debug = "debug";
        }

        public void addStoryBlock(string fileName)
        {

            XmlDocument doc = new XmlDocument();
            try {
                doc.Load(fileName);
                XmlElement rootElem = doc.DocumentElement;
                XmlNodeList title = rootElem.GetElementsByTagName("Title");
                XmlNodeList text = rootElem.GetElementsByTagName("Text");
                XmlNodeList img = rootElem.GetElementsByTagName("Image");
                Story s = new Story();
                s.image = new Image();
                s.image.url = img[0].InnerText;
                s.text = text[0].InnerText;
                s.title = title[0].InnerText;
                s.IsArchive = 0;
                s.UserName = null;
                db.Stories.Add(s);
                db.SaveChanges();
                System.IO.File.Delete(fileName);
            } catch(Exception ex) {
                //  doc.Load(collageXml);
            }
        }
    }
}
