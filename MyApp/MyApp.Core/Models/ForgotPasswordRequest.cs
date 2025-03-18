// Repositories/Models/ForgotPasswordRequest.cs
namespace MyApp.Core.Models
{
    public class ForgotPasswordRequest
    {
        public required string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        public required string Email { get; set; }
        public required string Token { get; set; }
        public required string NewPassword { get; set; }
    }

    public class PasswordResetToken
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string Token { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool Used { get; set; }
    }
}