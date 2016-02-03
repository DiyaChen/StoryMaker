using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.IO;

namespace My_lab7.Controllers
{
    public class FileUploadController : ApiController
    {
        [HttpPost]
        public string UploadFile()
        {
            string message = null;
            if (HttpContext.Current.Request.Files.AllKeys.Any()) {
                // Get the uploaded image from the Files collection
                var httpPostedFile = HttpContext.Current.Request.Files["UploadedImage"];
                if (httpPostedFile != null) {
                    // Validate the uploaded image(optional)
                    // Get the complete file path
                    var fileSavePath = Path.Combine(HttpContext.Current.Server.MapPath("~/CollagePages/UploadedFiles"), httpPostedFile.FileName);
                    // Save the uploaded file to "UploadedFiles" folder
                    httpPostedFile.SaveAs(fileSavePath);
                    message = "File uploaded as " + fileSavePath + " (" + httpPostedFile.ContentLength + " bytes)";
                }
            }
            return message;
        }
    }
}
