using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Collections.Generic;

namespace MyApp.MVC.Models
{
    [ApiController]
    [Route("api/admin")]
    public class AdminApiController : ControllerBase
    {
        private readonly string _connectionString = "Host=cipg01;Port=5432;Username=postgres;Password=123456;Database=Group_E_TaskTrack";

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
    }
}
