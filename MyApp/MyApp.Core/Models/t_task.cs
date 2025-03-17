using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyApp.Core.Models
{
    public class t_task
    {
        public int c_taskid { get; set; }
        public int c_userid { get; set; }
        public string c_title { get; set; }
        public string c_description { get; set; }
        public int c_estimateddays { get; set; }
        public DateTime c_startdate { get; set; }
        public DateTime c_enddate { get; set; }
        public string c_status { get; set; }  // Pending, In Progress, Completed
        public string c_document { get; set; } // File Path or URL
    }
}