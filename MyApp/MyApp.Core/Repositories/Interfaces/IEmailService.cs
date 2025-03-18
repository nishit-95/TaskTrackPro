// IEmailService.cs
using System.Threading.Tasks;
namespace Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}