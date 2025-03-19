using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyApp.Core.Model;
using MyApp.Core.Repositories.Interfaces;
using MyApp.Core.Models;
using MyApp.Core.Repositories.Interfaces;
using MyApp.Core.Services;
using Elastic.Clients.Elasticsearch.MachineLearning;
using Npgsql;

namespace MyApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserApiController : ControllerBase
    {
        private readonly IUserInterface _userServices;
        private readonly NpgsqlConnection _conn;

        private readonly ElasticSearchService _elasticSearchService;
        // private readonly RabbitMQService _rabbitMQService;
        private readonly RedisService _redisService;

        private readonly RabbitMQService _rabbitMQService;
        private readonly IUserInterface _userRepo;

        public UserApiController(ElasticSearchService elasticSearchService, RabbitMQService rabbitMQService, NpgsqlConnection conn, IUserInterface userServices, RedisService redisService, IUserInterface userRepo)
        {
            // RabbitMQService rabbitMQService
            _elasticSearchService = elasticSearchService;
            // _rabbitMQService = rabbitMQService;
            _userServices = userServices;
            _redisService = redisService;
            _rabbitMQService = rabbitMQService;
            _conn = conn;
            _userRepo = userRepo;
        }

        [HttpGet]
        [Route("GetTaskByUserId/{userId}")]
        #region task actions
        public async Task<IActionResult> GetTaskByUserId(int userId)
        {
            List<t_task_user> taskList = await _redisService.GetTaskList(userId);
            System.Console.WriteLine("from redis");
            if (taskList.Count == 0)
            {
                System.Console.WriteLine("form database");
                taskList = await _userServices.GetTaskByUserId(userId);
                if (taskList == null)
                {
                    return BadRequest("From User API Controller : There was some error while fetching the tasks");
                }
                _redisService.SetTaskList(userId, taskList);
            }
            return Ok(taskList);
        }

        [HttpGet]
        [Route("UpdateStatus/{taskId}/{userId}")]
        public async Task<IActionResult> UpdateStatus(int taskId, int userId)
        {
            System.Console.WriteLine(userId);
            var status = await _userServices.UpdateStatus(taskId);
            if (status == 0)
            {
                return BadRequest("From User API Controller : Status not updated");
            }
            else if (status == 1)
            {
                List<t_task_user> taskList = await _userServices.GetTaskByUserId(userId);
                _redisService.SetTaskList(userId, taskList);
                return Ok("Status Updated Successfully");
            }
            else
            {
                return BadRequest("From User API Controller : There was some error while updating the status");
            }
        }

        #endregion

        [HttpGet("search")]
        public async Task<IActionResult> SearchTasks([FromQuery] string query)
        {
            var tasks = await _elasticSearchService.SearchTasks(query);
            return Ok(tasks);
        }


        // -------For Testing-----------
        [HttpPost("registerQueue")]
        public IActionResult RegisterUser([FromBody] t_user user)
        {
            if (user == null)
                return BadRequest(new { error = "Invalid request: User object is null" });

            try
            {
                // Ensure connection is open
                if (_conn.State != System.Data.ConnectionState.Open)
                    _conn.Open();

                // Insert user into t_user table
                string insertSql = @"
            INSERT INTO t_user 
                (c_userName, c_email, c_password, c_address, c_mobile, c_gender, c_status, c_image) 
            VALUES 
                (@username, @email, @password, @address, @mobile, @gender, @status, @image)
            RETURNING c_userId;";

                using var cmd = new NpgsqlCommand(insertSql, _conn);
                cmd.Parameters.AddWithValue("@username", user.c_userName);
                cmd.Parameters.AddWithValue("@email", user.c_email);
                cmd.Parameters.AddWithValue("@password", user.c_password);
                cmd.Parameters.AddWithValue("@address", (object?)user.c_address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@mobile", (object?)user.c_mobile ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@gender", (object?)user.c_gender ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@status", (object?)user.c_status ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@image", (object?)user.c_image ?? DBNull.Value);

                var newUserId = cmd.ExecuteScalar();
                _conn.Close();

                if (newUserId == null)
                    return StatusCode(500, new { error = "Failed to register user" });

                // Create notification message


                int userId = Convert.ToInt32(newUserId);

                // Create notification message
                string message = $"New user registered: {user.c_userName}";

                // Publish notification with both message and userId
                _rabbitMQService.PublishMessage(message, userId);

                return Ok(new { message = "User registered successfully", userId = newUserId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("sendNotification/{taskTitle}/{userId}/{taskId}")]
        public async Task<IActionResult> sendNotification(string taskTitle, int userId, int taskId)
        {
            int result = await _userServices.SendNotification(taskTitle, userId, taskId);
            if (result == 0)
            {
                return BadRequest("Notificcation was not sent");
            }
            return Ok("Notification Sent successfully.");
        }
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromForm] t_User1 user)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { success = false, message = "Invalid input", errors });
            }

            try
            {
                if (user.ProfilePicture != null && user.ProfilePicture.Length > 0)
                {
                    if (user.ProfilePicture.Length > 5 * 1024 * 1024) // 5MB limit
                        return BadRequest(new { success = false, message = "Profile picture must not exceed 5MB" });

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var extension = Path.GetExtension(user.ProfilePicture.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                        return BadRequest(new { success = false, message = "Only .jpg, .jpeg, and .png files are allowed" });

                    var fileName = user.c_email + extension;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Profile_Pictures", fileName);
                    user.c_image = fileName;

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await user.ProfilePicture.CopyToAsync(stream);
                    }
                }
                else
                {
                    user.c_image = null;
                }
                user.c_role = "User";
                int userId = await _userRepo.Register(user); // Get the actual user ID

                if (userId > 0) // User registered successfully
                {
                    string message = $"New user registered: {user.c_userName}";
                    _rabbitMQService.PublishMessage(message, userId); // ✅ Now passing userId (int)

                    return Ok(new { success = true, message = "User Registered", userId });
                }
                else if (userId == 0)
                {
                    return Ok(new { success = false, message = "User Already Exists" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "There was some error during registration" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Server error: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] vm_Login user)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { success = false, message = "Invalid input", errors });
            }

            // Static Admin Login - Hardcoded admin handling
            if (user.c_email == "admin@example.com" && user.c_password == "Admin@123")
            {
                var adminUser = new t_User1
                {
                    c_userId = 1,
                    c_userName = "Admin",
                    c_email = "admin@example.com",
                    c_password = "[PROTECTED]",
                    c_role = "Admin",
                    c_status = "Active"
                };

                return Ok(new { success = true, message = "Login Successful", role = adminUser.c_role, UserData = adminUser });
            }

            // Fetch user from database
            t_User1 UserData = await _userRepo.Login(user);

            if (UserData == null || UserData.c_userId == 0)
            {
                var existingUser = await _userRepo.GetUserByEmail(user.c_email);
                if (existingUser != null && existingUser.c_status == "Pending")
                {
                    return Ok(new { success = false, message = "Your account is pending approval. Please wait for admin approval." });
                }
                return BadRequest(new { success = false, message = "Invalid email or password" });
            }

            return Ok(new { success = true, message = "Login Successful", role = UserData.c_role, UserData });
        }


        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { success = false, message = "Email is required" });
            }

            try
            {
                var result = await _userRepo.InitiatePasswordReset(request.Email);

                if (result.success)
                {
                    // In a production environment, you would NOT return the token
                    // But for development/testing, it's helpful
                    return Ok(new { success = true, message = result.message, token = result.token });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Server error: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest(new { success = false, message = "Email, token, and new password are required" });
            }

            try
            {
                var result = await _userRepo.ResetPassword(request.Email, request.Token, request.NewPassword);

                if (result.success)
                {
                    return Ok(new { success = true, message = result.message });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Server error: {ex.Message}" });
            }
        }





    }
}

