using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PTLogger.Models
{
    public class UserStoryModel
    {
        public int Id { get; set; }
        public int? ProjectId { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public List<LabelModel> Labels { get; set; }
    }
}