using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyApp.Core.Models
{
    public class t_user
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int c_userId { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "Username is required.")]
        public string? c_userName { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? c_email { get; set; }
        [StringLength(100)]
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string? c_password { get; set; }
       

        [StringLength(500)]
        public string? c_address { get; set; }
        [StringLength(50)]
        public string? c_mobile { get; set; }
        [StringLength(10)]
        public string? c_gender { get; set; }
        public string? c_status {get; set;}
        [StringLength(4000)]
        public string? c_image { get; set; }
    }
}