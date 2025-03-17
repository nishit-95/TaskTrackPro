using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyApp.Core.Repositories.Interfaces
{
    public interface IAdminInterface
    {
        Task<List<t_user>> GetAll();
         Task<List<t_task>> GetTaskDocument();
          int GetTotalUsers();
           int GetTotalTasks();
               Task<(int totalUsers, int totalTasks)> GetDashboardStatsAsync();
    }
}