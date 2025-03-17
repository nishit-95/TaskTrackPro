using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApp.Core.Models
{
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int c_notificationid { get; set; }
        public string c_title { get; set; }
        public int? c_taskid { get; set; }
        public int c_userid { get; set; }
        public bool c_isread { get; set; }
    }
}