using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PTLogger.Models
{
    public class NewUserStoryModel
    {
        public NewUserStoryModel()
        {
            Labels = new List<String>
            {
                "Support"
            };
        }
        
        public String Name { get; set; }
        public String Description { get; set; }
        public List<String> Labels { get; set; }
    }
}