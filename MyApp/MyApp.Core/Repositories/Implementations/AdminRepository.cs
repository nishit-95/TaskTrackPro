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

    }
}