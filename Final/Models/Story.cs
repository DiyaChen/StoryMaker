using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations;

namespace My_lab7.Models
{
    public class Story
    {
        //[Key]
        public int id { get; set; }
        public string title { get; set; }
        [DataType(DataType.MultilineText)]
        public string text { get; set; }
        public string DateOfBuild { get; set; }
        public string UserName { get; set; }
        public virtual Collage collage { get; set; }
        public virtual Image image { get; set; }
        public int IsArchive { get; set; }
    }

    public class Image
    {
        public int id {get; set; }
        public string url {get; set;}
    }

    public class Collage
    {
        //[Key]
        public int id { get; set; }
        public string name { get; set; }
        [DataType(DataType.MultilineText)]
        public string description { get; set; }
        public string DateOfBuild { get; set; }
        public string UserName { get; set; }
        public string PageLink { get; set; }

    }

}