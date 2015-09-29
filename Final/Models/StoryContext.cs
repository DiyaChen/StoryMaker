using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace My_lab7.Models
{
    public class StoryContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public StoryContext() : base("name=StoryContext")
        {
        }

        public System.Data.Entity.DbSet<My_lab7.Models.Story> Stories { get; set; }

        public System.Data.Entity.DbSet<My_lab7.Models.Collage> Collages { get; set; }

        public System.Data.Entity.DbSet<My_lab7.Models.Image> Images { get; set; }
    
    }
}
