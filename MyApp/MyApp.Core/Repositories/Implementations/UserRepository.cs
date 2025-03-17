using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Schema;
using MyApp.Core.Model;
using MyApp.Core.Repositories.Interfaces;
using Npgsql;

namespace MyApp.Core.Repositories.Implementations
{
    public class UserRepository : IUserInterface
    {
        private readonly NpgsqlConnection _conn;

        public UserRepository(NpgsqlConnection conn)
        {
            _conn = conn;
        }

        public async Task<List<t_task_user>> GetTaskByUserId(int userId)
        {
            List<t_task_user> TaskList = new List<t_task_user>();
            await _conn.CloseAsync();
            await _conn.OpenAsync();
            try
            {
                NpgsqlCommand GetTaskCmd = new NpgsqlCommand("SELECT * FROM t_task WHERE c_userid=@c_userid", _conn);
                GetTaskCmd.Parameters.AddWithValue("@c_userid", userId);
                NpgsqlDataReader datar = await GetTaskCmd.ExecuteReaderAsync();
                while (datar.Read())
                {
                    t_task_user task = new t_task_user
                    {
                        C_Taskid = datar.GetInt32(datar.GetOrdinal("c_taskid")),
                        C_Userid = datar.GetInt32(datar.GetOrdinal("c_userid")),
                        C_Title = datar.GetString(datar.GetOrdinal("c_title")),
                        C_Description = datar.GetString(datar.GetOrdinal("c_description")),
                        C_Estimateddays = datar.GetInt32(datar.GetOrdinal("c_estimateddays")),
                        C_Startdate = DateOnly.FromDateTime(datar.GetDateTime(datar.GetOrdinal("c_startdate"))),
                        C_Enddate = DateOnly.FromDateTime(datar.GetDateTime(datar.GetOrdinal("c_enddate"))),
                        C_Status = datar.GetString(datar.GetOrdinal("c_status")),
                        C_Document = datar.IsDBNull(datar.GetOrdinal("c_document")) ? null : datar.GetString(datar.GetOrdinal("c_document"))
                    };
                    TaskList.Add(task);
                }
                return TaskList;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("ERROR : From userrepo GetTaskByUserId method, There was some error while fetching the data : " + ex.Message);
                return TaskList;
            }
            finally
            {
                _conn.CloseAsync();
            }
        }

        public async Task<int> UpdateStatus(int taskId)
        {
            await _conn.CloseAsync();
            await _conn.OpenAsync();
            try
            {
                NpgsqlCommand updateStatusCmd = new NpgsqlCommand(@"UPDATE t_task 
                SET c_status = 'Completed' 
                WHERE c_taskid = @c_taskid", _conn);
                updateStatusCmd.Parameters.AddWithValue("@c_taskid", taskId);
                Object? statusObj = await updateStatusCmd.ExecuteNonQueryAsync();
                int status = (int)statusObj;
                if (status == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("ERROR : From userrepo UpdateStatus method, There was some error while Updating the status : " + ex.Message);
                return 2;
            }
            finally
            {
                await _conn.CloseAsync();
            }

        }

    }
}