using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyApp.Core.Model;
using MyApp.Core.Models;
using MyApp.MVC.Models;

namespace MyApp.Core.Repositories.Interfaces
{
    public interface IAdminInterface
    {
        List<User> GetAllUsers();
        Task<List<t_user>> GetAll();
        Task<List<t_task>> GetTaskDocument();
        Task<int> Add_Message(int senderId, string senderName, int receiverId, string receiverName, string messageKey);
        int GetTotalUsers();
        int GetTotalTasks();
        Task<(int totalUsers, int totalTasks)> GetDashboardStatsAsync();

         Task<List<object>>GetAllUser();

        // admin can assign task to user
        Task<int>AssignTask(TaskAssign taskAssign);

        // admin can update task to user
        Task<int>UpdateTask(TaskAssign taskAssign);

        // get all task
        Task<List<object>>GetAllTask();

        // get task by id
        Task<object>GetTaskById(int taskId);

        // task delete by id
        Task<int>DeleteTask(int taskId);
    }
}