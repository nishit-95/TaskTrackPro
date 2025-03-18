using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyApp.Core.Models;
using MyApp.Core.Services;
using Npgsql;

namespace MyApp.API.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class UserApiController : ControllerBase
    {
        private readonly NpgsqlConnection _conn;

        private readonly ElasticSearchService _elasticSearchService;
        private readonly RabbitMQService _rabbitMQService;

        public UserApiController(ElasticSearchService elasticSearchService, RabbitMQService rabbitMQService, NpgsqlConnection conn)
        {
            _elasticSearchService = elasticSearchService;
            _rabbitMQService = rabbitMQService;
            _conn = conn;
        }

        [HttpPost("index")]
        public async Task<IActionResult> IndexTask([FromBody] t_task task)
        {
            await _elasticSearchService.IndexTask(task);
            return Ok("Task indexed successfully.");
        }

        // ✅ Search & Filter Tasks
        [HttpGet("search")]
        public async Task<IActionResult> SearchTasks([FromQuery] string query, [FromQuery] string status = null)
        {
            var results = await _elasticSearchService.SearchTasks(query, status);
            return Ok(results);
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





    }
}