using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyApp.Core.Models
{
    public class vm_UserProfile
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int c_UserId { get; set; }

        public string c_UserName { get; set; }
        public string c_Email { get; set; }
        public string c_Password { get; set; }

        public string c_Mobile { get; set; }
        public string c_Gender { get; set; }
        public string c_Address { get; set; }
        public string c_Image { get; set; }
        public string c_Role{get; set; }
    }
}