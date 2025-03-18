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

                Task<int> SendNotification(string taskTitle, int userId, int taskId);
                Task<int> Register(t_User1 user);
                Task<t_User1> Login(vm_Login user);

                Task<t_User1> GetUserByEmail(string email);

                // Password reset methods
                Task<(bool success, string message, string token)> InitiatePasswordReset(string email);
                Task<(bool success, string message)> ResetPassword(string email, string token, string newPassword);
        }
}
