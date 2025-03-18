using MyApp.Core.Repositories.Implementations;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MyApp.Core.Models;
using MyApp.Core.Repositories.Interfaces;
using Npgsql;
using StackExchange.Redis;
using MyApp.MVC.Models;

namespace MyApp.Core.Repositories.Implementations
{
    public class AdminRepository : IAdminInterface
    {
        private readonly string _connectionString;

        public AdminRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT * FROM t_user", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            C_UserId = reader.GetInt32(0),
                            C_UserName = reader.GetString(1),
                            C_Email = reader.GetString(2),
                            C_Password = reader.GetString(3),
                            C_Mobile = reader.GetString(4),
                            C_Gender = reader.GetString(5),
                            C_Address = reader.GetString(6),
                            C_Status = reader.GetString(7),
                            C_Image = reader.IsDBNull(8) ? null : reader.GetString(8)
                        });
                    }
                }
            }
            return users;
        }


        // List<IAdminInterface> GetAllUsers()
        // {
        //     throw new NotImplementedException();
        // }
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly NpgsqlConnection _conn;

        public AdminRepository(IConnectionMultiplexer redis, NpgsqlConnection connection)
        {
            _redis = redis;
            _db = _redis.GetDatabase();
            _conn = connection;
        }
        public async Task<List<t_user>> GetAll()
        {
            DataTable dt = new DataTable();
            NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM t_user WHERE c_status = @status", _conn);
            cmd.Parameters.AddWithValue("@status", "Pending");
            _conn.Close();
            _conn.Open();
            NpgsqlDataReader datar = cmd.ExecuteReader();
            if (datar.HasRows)
            {
                dt.Load(datar);
            }
            List<t_user> contactList = new List<t_user>();
            contactList = (from DataRow dr in dt.Rows
                           select new t_user()
                           {
                               c_userId = Convert.ToInt32(dr["c_userId"]),
                               c_userName = dr["c_userName"].ToString(),
                               c_email = dr["c_email"].ToString(),
                               c_mobile = dr["c_mobile"].ToString(),
                               c_gender = dr["c_gender"].ToString(),
                               c_address = dr["c_address"].ToString(),
                               c_image = dr["c_image"].ToString(),
                               c_status = dr["c_status"].ToString()
                           }).ToList();
            _conn.Close();
            return contactList;
        }

        public async Task<List<t_task>> GetTaskDocument()
        {
            List<t_task> taskList = new List<t_task>();

            string query = @"
        SELECT 
         t.c_taskid, 
            t.c_title, 
            t.c_description, 
            t.c_document, 
            t.c_status, 

            u.c_userName 
        FROM t_task t
        INNER JOIN t_user u ON t.c_userid = u.c_userid
         WHERE t.c_document IS NOT NULL";

            using (NpgsqlCommand cmd = new NpgsqlCommand(query, _conn))
            {
                _conn.Close();
                _conn.Open();

                using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        taskList.Add(new t_task
                        {
                            c_title = reader["c_title"].ToString(),
                            c_description = reader["c_description"].ToString(),
                            c_document = reader["c_document"].ToString(),
                            c_userName = reader["c_userName"].ToString()
                        });
                    }
                }

                _conn.Close();
            }

            return taskList;
        }

        public int GetTotalUsers()
        {
            int count = 0;
            _conn.Close();
            _conn.Open();
            using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM t_user;", _conn))
            {
                count = Convert.ToInt32(command.ExecuteScalar());
            }
            _conn.Close();
            return count;
        }

        public int GetTotalTasks()
        {
            int count = 0;
            _conn.Close();
            _conn.Open();
            using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM t_task;", _conn))
            {
                count = Convert.ToInt32(command.ExecuteScalar());
            }

            return count;
        }
        public async Task<(int totalUsers, int totalTasks)> GetDashboardStatsAsync()
        {
            // 🔍 Step 1: Check if Redis has correct values
            string cachedData = await _db.StringGetAsync("dashboard_stats");
            Console.WriteLine("Redis Cached Data: " + cachedData);

            if (!string.IsNullOrEmpty(cachedData))
            {
                var stats = JsonSerializer.Deserialize<DashboardStats>(cachedData);
                Console.WriteLine($"Returning Cached Data: Users = {stats.totalUsers}, Tasks = {stats.totalTasks}");
                return (stats.totalUsers, stats.totalTasks);
            }

            // 🔍 Step 2: Fetch Fresh Data from PostgreSQL
            await _conn.OpenAsync();
            var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM t_user", _conn);
            int totalUsers = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            cmd = new NpgsqlCommand("SELECT COUNT(*) FROM t_task", _conn);
            int totalTasks = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            await _conn.CloseAsync();

            Console.WriteLine($"Fetched Fresh Data: Users = {totalUsers}, Tasks = {totalTasks}");

            // 🔍 Step 3: Store Updated Data in Redis
            var statsObj = new DashboardStats { totalUsers = totalUsers, totalTasks = totalTasks };
            await _db.StringSetAsync("dashboard_stats", JsonSerializer.Serialize(statsObj), TimeSpan.FromMinutes(5));

            Console.WriteLine($"Stored in Redis: Users = {totalUsers}, Tasks = {totalTasks}");

            return (totalUsers, totalTasks);
        }

        public class DashboardStats
        {
            public int totalUsers { get; set; }
            public int totalTasks { get; set; }
        }



        // viral
        public async Task<List<object>> GetAllUser()
        {
            try
            {
                await using (var cmd = new NpgsqlCommand("SELECT * FROM t_user", _conn))
                {
                    await _conn.OpenAsync();
                    await using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        var list = new List<object>();
                        while (await reader.ReadAsync())
                        {
                            list.Add(new
                            {
                                UserId = reader.GetInt32(0),
                                UserName = reader.GetString(1),
                            });
                        }
                        return list;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting users: {ex.Message}", ex);
                return null;
            }
            finally
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }

        public async Task<int> AssignTask(TaskAssign taskAssign)
        {
            try
            {
                await _conn.OpenAsync();

                int taskId;
                using (var cmd = new NpgsqlCommand("INSERT INTO t_task (c_userId, c_title, c_description, c_estimatedDays, c_startDate, c_endDate, c_status, c_document) VALUES (@userId, @title, @description, @estimatedDays, @startDate, @endDate, @status, @document) RETURNING c_taskId", _conn))
                {
                    cmd.Parameters.AddWithValue("userId", taskAssign.UserId);
                    cmd.Parameters.AddWithValue("title", taskAssign.Title);
                    cmd.Parameters.AddWithValue("description", taskAssign.Description);
                    cmd.Parameters.AddWithValue("estimatedDays", taskAssign.EstimatedDays);
                    cmd.Parameters.AddWithValue("startDate", taskAssign.StartDate);
                    cmd.Parameters.AddWithValue("endDate", taskAssign.EndDate);
                    cmd.Parameters.AddWithValue("status", taskAssign.Status);
                    cmd.Parameters.AddWithValue("document", DBNull.Value).NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Text;

                    taskId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                // Insert notification, but even if it fails, task insertion is already done
                try
                {
                    using (var notificationCmd = new NpgsqlCommand("INSERT INTO t_notification (c_title, c_taskId, c_userId, c_isread) VALUES (@title, @taskId, @userId, @isRead)", _conn))
                    {
                        notificationCmd.Parameters.AddWithValue("title", $"New Task Assigned: {taskAssign.Title}");
                        notificationCmd.Parameters.AddWithValue("taskId", taskId);
                        notificationCmd.Parameters.AddWithValue("userId", taskAssign.UserId);
                        notificationCmd.Parameters.AddWithValue("isRead", false); // Default unread

                        await notificationCmd.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception ex)
                {
                    // Log notification error, but do not rollback task assignment
                    Console.WriteLine($"Error inserting notification: {ex.Message}");
                }

                return taskId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error assigning task: {ex.Message}", ex);
            }
            finally
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }
        public async Task<int> UpdateTask(TaskAssign taskAssign)
        {
            try
            {
                await _conn.OpenAsync();

                int userId = 0;

                // Check if the task exists and fetch the userId
                using (var cmd = new NpgsqlCommand("SELECT c_userId FROM t_task WHERE c_taskId = @taskId", _conn))
                {
                    cmd.Parameters.AddWithValue("taskId", taskAssign.TaskId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync())
                            return 0; // Task does not exist

                        userId = reader.GetInt32(0); // Fetch the associated userId
                    }
                }

                // Update the task details
                using (var cmd = new NpgsqlCommand("UPDATE t_task SET c_title = @title, c_description = @description, c_estimatedDays = @estimatedDays, c_startDate = @startDate, c_endDate = @endDate, c_status = @status WHERE c_taskId = @taskId", _conn))
                {
                    cmd.Parameters.AddWithValue("taskId", taskAssign.TaskId);
                    cmd.Parameters.AddWithValue("title", taskAssign.Title);
                    cmd.Parameters.AddWithValue("description", taskAssign.Description);
                    cmd.Parameters.AddWithValue("estimatedDays", taskAssign.EstimatedDays);
                    cmd.Parameters.AddWithValue("startDate", taskAssign.StartDate);
                    cmd.Parameters.AddWithValue("endDate", taskAssign.EndDate);
                    cmd.Parameters.AddWithValue("status", taskAssign.Status);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        // Insert notification for task update
                        try
                        {
                            using (var notificationCmd = new NpgsqlCommand("INSERT INTO t_notification (c_title, c_taskId, c_userId, c_isread) VALUES (@title, @taskId, @userId, @isRead)", _conn))
                            {
                                notificationCmd.Parameters.AddWithValue("title", $"Task Updated: {taskAssign.Title}");
                                notificationCmd.Parameters.AddWithValue("taskId", taskAssign.TaskId);
                                notificationCmd.Parameters.AddWithValue("userId", userId); // Use the fetched userId
                                notificationCmd.Parameters.AddWithValue("isRead", false); // Default unread

                                await notificationCmd.ExecuteNonQueryAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error inserting notification: {ex.Message}");
                        }
                    }
                    return rowsAffected;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating task: {ex.Message}", ex);
            }
            finally
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }
        public async Task<List<object>> GetAllTask()
        {
            const string query = @"
                SELECT t.*, u.c_userName 
                FROM t_task t 
                LEFT JOIN t_user u ON t.c_userId = u.c_userId";
            try
            {
                await using (var cmd = new NpgsqlCommand(query, _conn))
                {
                    await _conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        var list = new List<object>();
                        while (await reader.ReadAsync())
                        {
                            list.Add(new
                            {
                                TaskId = reader.GetInt32(0),
                                UserId = reader.GetInt32(1),
                                Title = reader.GetString(2),
                                Description = reader.GetString(3),
                                EstimatedDays = reader.GetInt32(4),
                                StartDate = reader.GetDateTime(5),
                                EndDate = reader.GetDateTime(6),
                                Status = reader.GetString(7),
                                Document = reader.IsDBNull(8) ? null : reader.GetString(8),
                                UserName = reader.GetString(9)
                            });
                        }
                        return list;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting tasks: {ex.Message}", ex);
                return null;
            }
            finally
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }

        public async Task<object> GetTaskById(int taskId)
        {
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT * FROM t_task WHERE c_taskId = @taskId", _conn))
                {
                    cmd.Parameters.AddWithValue("taskId", taskId);
                    await _conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new
                            {
                                TaskId = reader.GetInt32(0),
                                UserId = reader.GetInt32(1),
                                Title = reader.GetString(2),
                                Description = reader.GetString(3),
                                EstimatedDays = reader.GetInt32(4),
                                StartDate = reader.GetDateTime(5),
                                EndDate = reader.GetDateTime(6),
                                Status = reader.GetString(7),
                                Document = reader.IsDBNull(8) ? null : reader.GetString(8)
                            };
                        }
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting task: {ex.Message}", ex);
                return null;
            }
            finally
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }

        public async Task<int> DeleteTask(int taskId)
        {
            try
            {
                using (var cmd = new NpgsqlCommand("DELETE FROM t_task WHERE c_taskId = @taskId", _conn))
                {
                    cmd.Parameters.AddWithValue("taskId", taskId);
                    await _conn.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting task: {ex.Message}", ex);
                return 0;
            }
            finally
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }

        public async Task<int> Add_Message(int SenderId, string senderName, int ReceiverId, string receiverName, string messageKey)
        {
            Console.WriteLine(senderName + " " + receiverName + " " + messageKey);
            try
            {
                int senderId = SenderId, receiverId = ReceiverId;
                using (NpgsqlCommand cmd = new NpgsqlCommand("Insert into t_message(c_senderid, c_receiverid, c_sendername, c_receivername, c_message_key) values(@senderId, @receiverId, @sendername, @receivername,  @messageKey)", _conn))
                {
                    cmd.Parameters.AddWithValue("@senderId", senderId);
                    cmd.Parameters.AddWithValue("@receiverId", receiverId);
                    cmd.Parameters.AddWithValue("@sendername", senderName);
                    cmd.Parameters.AddWithValue("@receivername", receiverName);
                    cmd.Parameters.AddWithValue("@messageKey", messageKey);
                    await _conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                    await _conn.CloseAsync();
                }
                Console.WriteLine("Sender Id: " + senderId + " Receiver Id: " + receiverId + "Sender name : " + senderName + " Receiver name : " + receiverName + " Message Key : " + messageKey);

                return 1;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred while adding message to the database in user repository." + ex.Message);
                return 0;
            }
        }

    }
}