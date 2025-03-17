using MyApp.Core.Repositories.Implementations;
using Npgsql;

namespace MyApp.MVC.Models
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
    }
}