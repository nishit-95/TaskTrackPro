using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyApp.Core.Model;

namespace MyApp.Core.Repositories.Interfaces
{
    public interface IUserInterface
    {
        Task<List<t_task_user>> GetTaskByUserId(int userId);

        Task<int> UpdateStatus(int taskId);
    }
}
