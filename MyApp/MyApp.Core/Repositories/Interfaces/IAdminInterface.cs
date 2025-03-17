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
        int GetTotalUsers();
        int GetTotalTasks();
        Task<(int totalUsers, int totalTasks)> GetDashboardStatsAsync();
    }
}