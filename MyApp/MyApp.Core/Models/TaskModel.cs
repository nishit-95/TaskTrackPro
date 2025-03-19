using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyApp.Core.Models
{
    public class TaskModel
    {
        public int TaskId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string HighlightedTitle { get; set; }
    }
}