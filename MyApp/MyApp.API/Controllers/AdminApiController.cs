using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyApp.Core.Repositories.Interfaces;
using Npgsql;
using StackExchange.Redis;
using MyApp.Core.Models;

namespace MyApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class AdminApiController : ControllerBase
    {
        private readonly IAdminInterface _admin;
        private readonly ConnectionMultiplexer _redis;
        private readonly string _connectionString;


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


        // viral
        
        [HttpGet("GetAllUser")]
        public async Task<IActionResult> GetAllUser()
        {
            try
            {
                var users = await _admin.GetAllUser();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // admin can assign task to user
        [HttpPost("AssignTask")]
        public async Task<IActionResult> AssignTask([FromBody] TaskAssign taskAssign)
        {
            try
            {
                var result = await _admin.AssignTask(taskAssign);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateTask")]
        public async Task<IActionResult> UpdateTask([FromBody] TaskAssign taskAssign)
        {
            try
            {
                var result = await _admin.UpdateTask(taskAssign);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetAllTask")]
        public async Task<IActionResult> GetAllTask()
        {
            try
            {
                var tasks = await _admin.GetAllTask();
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetTaskById/{taskId}")]
        public async Task<IActionResult> GetTaskById(int taskId)
        {
            try
            {
                var task = await _admin.GetTaskById(taskId);
                return Ok(task);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeleteTask/{taskId}")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            try
            {
                var result = await _admin.DeleteTask(taskId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        

    }
}