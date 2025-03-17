using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Collections.Generic;
using MyApp.Core.Repositories.Interfaces;
using Npgsql;
using StackExchange.Redis;
using MyApp.MVC.Models;

namespace MyApp.API.Controllers
{
    [ApiController]
    [Route("api/AdminApi")]
    public class AdminApiController : ControllerBase
    {
        private readonly IAdminInterface _admin;
        private readonly ConnectionMultiplexer _redis;
        private readonly string _connectionString;

        // ✅ GET: api/admin/users - Returns JSON list of users
        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            var users = new List<User>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT * FROM t_user ORDER BY c_userId", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            C_UserId = reader.GetInt32(0), // ✅ Fixed PascalCase Naming
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

            if (users.Count == 0)
                return NotFound(new { message = "No users found!" });

            return Ok(users); // ✅ Returns JSON response (fixes StackOverflow)
        }

        public AdminApiController(IConfiguration configuration, IAdminInterface admin)
        {
            _admin = admin;
            _connectionString = configuration.GetConnectionString("pgconn");
        }


        [HttpGet]
        [Route("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            string userid = HttpContext.Request.Query["userid"].ToString();


            var list = await _admin.GetAll();

            return Ok(list);
        }

        [HttpGet]
        [Route("GetDoc")]
        public async Task<IActionResult> GetAllDoc()
        {
            string userid = HttpContext.Request.Query["userid"].ToString();


            var list = await _admin.GetTaskDocument();

            return Ok(list);
        }

        [HttpPost("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus([FromBody] UserStatusUpdateModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Status))
            {
                return BadRequest("Invalid data");
            }

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            string sql = "UPDATE t_user SET c_status = @status WHERE c_userId = @userId";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@status", model.Status);
            cmd.Parameters.AddWithValue("@userId", model.UserId);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                return Ok(new { message = "User status updated successfully." });
            }
            else
            {
                return NotFound(new { message = "User not found." });
            }
        }

        // Model for status update
        public class UserStatusUpdateModel
        {
            public int UserId { get; set; }
            public string Status { get; set; }
        }

        [HttpGet("pendingUserCount")]
        public async Task<IActionResult> GetPendingUserCount()
        {
            int count = 0;
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            string sql = "SELECT COUNT(*) FROM t_user WHERE c_status = 'Pending'";
            using var cmd = new NpgsqlCommand(sql, conn);
            count = Convert.ToInt32(cmd.ExecuteScalar());

            return Ok(count);
        }

        [HttpGet("notifications")]
        public IActionResult GetNotifications()
        {
            List<object> notifications = new List<object>();

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            string sql = "SELECT c_notificationId, c_title FROM t_notification WHERE c_isRead = FALSE AND c_title ILIKE '%registered%' ORDER BY c_notificationId DESC";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                notifications.Add(new
                {
                    id = reader.GetInt32(0), // Notification ID
                    title = reader.GetString(1) // Notification text
                });
            }

            return Ok(notifications);
        }


        [HttpPost("mark-notification-read/{id}")]
        public IActionResult MarkNotificationAsRead(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            string sql = "UPDATE t_notification SET c_isRead = TRUE WHERE c_notificationId = @id";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                return Ok(new { message = "Notification marked as read" });
            }
            else
            {
                return NotFound(new { message = "Notification not found" });
            }
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var (totalUsers, totalTasks) = await _admin.GetDashboardStatsAsync();
            Console.WriteLine($"Total Users: {totalUsers}, Total Tasks: {totalTasks}"); // Debugging

            return Ok(new { totalUsers, totalTasks });
        }

    }
}
