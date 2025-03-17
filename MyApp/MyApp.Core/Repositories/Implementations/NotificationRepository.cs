using Npgsql;
using MyApp.Core.Models;
using MyApp.Core.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MyApp.Core.Repositories
{
    public class NotificationRepository : INotification
    {
        private readonly string _connectionString;

        public NotificationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("pgConn");
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new ArgumentNullException(nameof(_connectionString), "Connection string 'pgConn' is not configured in appsettings.json.");
            }
        }

        public async Task CreateNotification(Notification notification)
        {
            using var con = new NpgsqlConnection(_connectionString);
            await con.OpenAsync();
            var query = "INSERT INTO t_notification (c_title, c_taskid, c_userid, c_isread) VALUES (@c_title, @c_taskid, @c_userid, @c_isread)";
            using var command = new NpgsqlCommand(query, con);
            command.Parameters.AddWithValue("c_title", notification.c_title);
            command.Parameters.AddWithValue("c_taskid", (object)notification.c_taskid ?? DBNull.Value);
            command.Parameters.AddWithValue("c_userid", notification.c_userid);
            command.Parameters.AddWithValue("c_isread", notification.c_isread);
            await command.ExecuteNonQueryAsync();
        }

        public async Task<List<Notification>> GetNotificationsByUserId(int userId)
        {
            var notifications = new List<Notification>();
            using var con = new NpgsqlConnection(_connectionString);
            await con.OpenAsync();
            var query = "SELECT c_notificationid, c_title, c_taskid, c_userid, c_isread FROM t_notification WHERE c_userid = @c_userid ORDER BY c_notificationid DESC";
            using var command = new NpgsqlCommand(query, con);
            command.Parameters.AddWithValue("c_userid", userId);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                notifications.Add(new Notification
                {
                    c_notificationid = reader.GetInt32(0),
                    c_title = reader.GetString(1),
                    c_taskid = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                    c_userid = reader.GetInt32(3),
                    c_isread = reader.GetBoolean(4)
                });
            }
            return notifications;
        }

        public async Task<int> GetUnreadNotificationCount(int userId)
        {
            using var con = new NpgsqlConnection(_connectionString);
            await con.OpenAsync();
            var query = "SELECT COUNT(*) FROM t_notification WHERE c_userid = @c_userid AND c_isread = FALSE";
            using var command = new NpgsqlCommand(query, con);
            command.Parameters.AddWithValue("c_userid", userId);
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }





        public async Task MarkAsRead(int notificationId)
        {
            using var con = new NpgsqlConnection(_connectionString);
            await con.OpenAsync();
            var query = "UPDATE t_notification SET c_isread = TRUE WHERE c_notificationid = @c_notificationid";
            using var command = new NpgsqlCommand(query, con);
            command.Parameters.AddWithValue("c_notificationid", notificationId);
            await command.ExecuteNonQueryAsync();
        }


    }
}