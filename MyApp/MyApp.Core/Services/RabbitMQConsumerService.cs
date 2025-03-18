using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MyApp.Core.Services
{
    public class RabbitMQConsumerService : BackgroundService
    {
        private readonly string _queueName = "UserRegistrationQueue";
        private readonly ConnectionFactory _factory;
        private IConnection _connection;
        private IModel _channel;
        private readonly string _connectionString;

        public RabbitMQConsumerService(IConfiguration configuration)
        {
            _factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            _connectionString = configuration.GetConnectionString("pgconn");
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var jsonMessage = Encoding.UTF8.GetString(body);

                Console.WriteLine($"[Notification Received]: {jsonMessage}");

                try
                {
                    // 🔹 Deserialize JSON
                    var notificationData = JsonConvert.DeserializeObject<NotificationModel>(jsonMessage);
                    if (notificationData != null)
                    {
                        SaveNotificationToDatabase(notificationData.Title, notificationData.UserId);
                    }
                    else
                    {
                        Console.WriteLine("[Error] Unable to parse JSON message.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Deserialization Error]: {ex.Message}");
                }
            };

            _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);

            return Task.CompletedTask;
        }

        private void SaveNotificationToDatabase(string title, int userId)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                conn.Open();

                string sql = "INSERT INTO t_notification (c_title, c_userId) VALUES (@title, @userId)";

                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@title", title);
                cmd.Parameters.AddWithValue("@userId", userId);

                cmd.ExecuteNonQuery();

                Console.WriteLine($"[Database] Notification saved: Title={title}, UserId={userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Database Error]: {ex.Message}");
            }
        }

        public class NotificationModel
        {
            public string Title { get; set; }
            public int UserId { get; set; }
        }


        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }

    }
}