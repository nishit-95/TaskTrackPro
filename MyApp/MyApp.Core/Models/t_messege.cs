using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyApp.Core.Models
{
    public class t_messege
    {
        public int SenderId { get; set; }

        [Required]
        [StringLength(50)]
        public string SenderName { get; set; }

        public int ReceiverId { get; set; }

        [Required]
        [StringLength(50)]
        public string ReceiverName { get; set; }

        [Required]
        [StringLength(500)]
        public string MessageKey { get; set; }
    }
}