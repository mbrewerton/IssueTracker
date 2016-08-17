using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PTLogger.Models
{
    public class NewCommentModel
    {
        public string Text { get; set; }
        public int Project_Id { get; set; }
        public int Story_Id { get; set; }
        public List<AttachmentModel> File_Attachments { get; set; }
    }
}