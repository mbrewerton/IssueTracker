using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PTLogger.Models
{
    public class CommentModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int ProjectId { get; set; }
        public int StoryId { get; set; }
    }
}