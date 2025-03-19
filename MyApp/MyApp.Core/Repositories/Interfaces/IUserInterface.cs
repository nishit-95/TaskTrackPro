using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyApp.Core.Model;
using MyApp.Core.Models;

namespace MyApp.Core.Repositories.Interfaces
{
    public interface IUserInterface
    {
        Task<List<t_task_user>> GetTaskByUserId(int userId);

        Task<int> UpdateStatus(int taskId);
        Task<int> Register(t_User1 user);
        Task<t_User1> Login(vm_Login user);

        Task<t_User1> GetUserByEmail(string email);

        Task<List<t_Notification>> GetNotificationsByUserIdAsync(int userId);
        Task<int> GetUnreadNotificationCount(int userId);
        Task MarkNotificationAsRead(int userId, int notificationId);
        // Task<t_Notification> GetNotificationByIdAsync(int notificationId);
        Task<t_task> GetTaskByNotificationAsync(int notificationId);
        
        // Password reset methods
        Task<(bool success, string message, string token)> InitiatePasswordReset(string email);
        Task<(bool success, string message)> ResetPassword(string email, string token, string newPassword);
    }
}
