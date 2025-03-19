using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Collections.Generic;
using MyApp.Core.Repositories.Interfaces;
using Npgsql;
using StackExchange.Redis;
using MyApp.Core.Models;
using MyApp.MVC.Models;
using MyApp.Core.Services;
using MyApp.Core.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace MyApp.API.Controllers
{
    [ApiController]
    [Route("api/AdminApi")]
    public class AdminApiController : ControllerBase
    {
        private readonly IRabbitMQService rabbitMQService;
        private readonly IRedisService redisService;
        private readonly IAdminInterface _admin;
        private readonly ConnectionMultiplexer _redis;
        private readonly string _connectionString;
        private readonly MRabbitMQService _rabbitMQService;
        private readonly MRedisService _redisService;
        private readonly IHubContext<NotificationHub> _hubContext;


        public AdminApiController(IConfiguration configuration, IAdminInterface admin, IRabbitMQService _rabbitMQService, IRedisService _redisService)
        {
            _admin = admin;
            this.rabbitMQService = _rabbitMQService;
            this.redisService = _redisService;
            _connectionString = configuration.GetConnectionString("pgconn");
        }

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
        // [HttpPost("AssignTask")]
        // public async Task<IActionResult> AssignTask([FromBody] TaskAssign taskAssign)
        // {
        //     try
        //     {
        //         var result = await _admin.AssignTask(taskAssign);
        //         return Ok(result);
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(ex.Message);
        //     }
        // }

        [HttpPost("AssignTask")]
public async Task<IActionResult> AssignTask([FromBody] TaskAssign taskAssign)
{
    try
    {
        // Assign the task
        var result = await _admin.AssignTask(taskAssign);

        // Create notification object
        var notification = new t_Notification
        {
            TaskId = taskAssign.TaskId,
            UserId = taskAssign.UserId,
            Title = $"New Task Assigned: {taskAssign.Title}",
            IsRead = false,  // Default to unread
            CreatedAt = DateTime.UtcNow
        };

        // Save notification to DB
        await _admin.AddNotification(notification);

        // Construct detailed notification message
        string notificationMessage = $"Task Assigned: {taskAssign.Title}\n" +
                                     $"Description: {taskAssign.Description}\n" +
                                     $"Start Date: {taskAssign.StartDate:yyyy-MM-dd}\n" +
                                     $"End Date: {taskAssign.EndDate:yyyy-MM-dd}\n" +
                                     $"Estimated Days: {taskAssign.EstimatedDays}\n" +
                                     $"Status: {taskAssign.Status}";

        // Send notification via SignalR to the specific user
        await _hubContext.Clients.User(notification.UserId.ToString())
            .SendAsync("ReceiveNotification", notificationMessage);

        // Cache notification in Redis
        await _redisService.CacheNotifications(notification.UserId, new List<t_Notification> { notification });

        // Publish notification to Redis pub-sub
        _redisService.PublishNotification(notification.UserId.ToString(), notificationMessage);

        // Send notification via RabbitMQ
        _rabbitMQService.PublishMessageTask(notification.UserId, notification);

        return Ok(new { message = $"Task assigned to user {notification.UserId}, notification sent!", result });
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



        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromQuery] string queueName, [FromQuery] string Receiver, [FromBody] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return BadRequest("Message cannot be null or empty.");
            }
            if (string.IsNullOrEmpty(Receiver))
            {
                return BadRequest("Receiver cannot be null or empty.");
            }
            if (string.IsNullOrEmpty(queueName))
            {
                return BadRequest("Sender cannot be null or empty.");
            }

            // Apde ahiya redis ma message store karvaye chiye with the help of message key which will be unique for each message
            string redisKey = $"message:{Receiver}:{Guid.NewGuid()}";
            redisService.Set(redisKey, message);

            // Ane aee unique key data base ma store karavye chiye 
            // int senderId = await userInterface.GetUserId(queueName);
            int senderId = 0;
            int receiverId = 0;
            // int receiverId = await userInterface.GetUserId(Receiver);
            int result = await _admin.Add_Message(senderId, queueName, receiverId, Receiver, redisKey);
            // Console.WriteLine("Result : " + result);

            // Get the sender's username (you can pass this from the frontend or use the session)
            var receiver = Receiver; // Or use session/context to get the sender's username
            // Console.WriteLine($"Sender: {receiver}");

            // Send the message with the sender's username
            rabbitMQService.SendMessage(queueName, receiver, message);
            return Ok("Message sent successfully, and saved in Redis with key : " + redisKey);
        }

        

        [HttpGet("receive")]
        public async Task<IActionResult> ReceiveMessage([FromQuery] string queueName, [FromQuery] string redisKey)
        {
            var (sender, message) = rabbitMQService.ReceiveMessage(queueName);
            if (sender == null || message == null)
            {
                return NotFound("No messages available.");
            }
            string result = await redisService.Get(redisKey);
            // Console.WriteLine("Result : " + result);

            // Return a structured response
            return Ok(new { Sender = sender, Message = message });
        }



    }
}
