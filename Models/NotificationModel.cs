using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS_Notification.Models
{
    public class NotificationModel
    {
        public string Subject { get; set; }
        public string Text { get; set; }
        public string SendTo { get; set; }
        public DateTime SentDate { get; set; }
        public int SentStatus { get; set; }
        public string StatusMessage { get; set; }
    }
}


