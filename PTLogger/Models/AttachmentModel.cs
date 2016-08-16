using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PTLogger.Models
{
    public class AttachmentModel
    {
        public string FileName { get; set; }
        public int Id { get; set; }
        public int UploaderId { get; set; }
    }
}