using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace MyApp.Core.Models
{
public class t_User1
{
    [Key]
     [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

    public int c_userId { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 200 characters")]
    public string c_userName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email must not exceed 100 characters")]
    public string c_email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Password must contain at least one letter and one number")]
    public string c_password { get; set; }

    [StringLength(20, MinimumLength = 10, ErrorMessage = "Mobile number must be between 10 and 20 digits")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Mobile number must contain only digits")]
    public string c_mobile { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    [StringLength(10, ErrorMessage = "Gender must not exceed 10 characters")]
    public string c_gender { get; set; }

    [StringLength(400, ErrorMessage = "Address must not exceed 400 characters")]
    public string c_address { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [StringLength(20, ErrorMessage = "Status must not exceed 20 characters")]
    public string c_status { get; set; } = "Pending";

    [StringLength(4000, ErrorMessage = "Image path must not exceed 4000 characters")]
    public string? c_image { get; set; }

    [Required(ErrorMessage = "Role is required")]
    [StringLength(20, ErrorMessage = "Role must not exceed 20 characters")]
    public string c_role { get; set; } = "User";

    [NotMapped] // Since this is for file upload, not stored directly in DB
    public IFormFile? ProfilePicture { get; set; }
}
}