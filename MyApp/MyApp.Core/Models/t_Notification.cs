using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyApp.Core.Models
{
    public class t_Notification
    {
        public int NotificationId { get; set; }
        public string Title { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}