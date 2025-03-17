using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyApp.Core.Model;
using MyApp.Core.Repositories.Interfaces;
using Npgsql;

namespace MyApp.Core.Repositories.Implementations
{
    public class UserRepository : IUserInterface
    {
        // private readonly NpgsqlConnection _conn;

        // public UserRepository(NpgsqlConnection conn)
        // {
        //     _conn = conn;
        // }

        // public async Task<List<t_task_user>> GetTaskByUserId(int userId)
        // {
        //     List<t_task_user> TaskList = new List<t_task_user>();
        //     try
        //     {
        //         _conn.CloseAsync();
        //         NpgsqlCommand GetTaskCmd = new NpgsqlCommand("SELECT * FROM t_task WHERE c_userid=@c_userid", _conn);
        //         GetTaskCmd.Parameters.AddWithValue("",userId);
        //     }
        // }
    }
}