namespace MyApp.MVC.Models
{
    public interface IAdminInterface
    {
        List<User> GetAllUsers();
    }

    // public class User
    // {
    //     public int C_UserId { get; set; }
    //     public string C_UserName { get; set; }
    //     public string C_Email { get; set; }
    //     public string C_Password { get; set; }
    //     public string C_Mobile { get; set; }
    //     public string C_Gender { get; set; }
    //     public string C_Address { get; set; }
    //     public string C_Status { get; set; }
    //     public string? C_Image { get; set; }
    // }
}