using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyApp.Core.Models;
using MyApp.Core.Repositories.Interfaces;
using MyApp.Core.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MyApp.Core.BackgroundServices
{
    public class RabbitMQListenerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConnectionFactory _factory;
        private IConnection _connection;
        private IModel _channel;
        // private readonly List<string> _userQueues = new List<string>();
        public RabbitMQListenerService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _factory = new ConnectionFactory() { HostName = "localhost" };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            // channel.QueueDeclare(queue: "notification_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

            // var consumer = new EventingBasicConsumer(channel);
            // consumer.Received += async (model, ea) =>
            // {
            //     var body = ea.Body.ToArray();
            //     var message = Encoding.UTF8.GetString(body);
            //     var notification = JsonConvert.DeserializeObject<t_Notification>(message);

            //     using var scope = _scopeFactory.CreateScope();
            //     var adminRepo = scope.ServiceProvider.GetRequiredService<IAdminInterface>();
            //     var redisService = scope.ServiceProvider.GetRequiredService<RedisService>();

            //     // Save notification to the database
            //     await adminRepo.AddNotification(notification);

            //     // Cache and publish notification in Redis
            //     await redisService.CacheNotifications(notification.UserId, new List<t_Notification> { notification });
            //     redisService.PublishNotification(notification.UserId.ToString(), notification.Title);

            //     Console.WriteLine($"[RabbitMQ] Notification processed for user {notification.UserId}: {notification.Title}");
            // };

            // channel.BasicConsume(queue: "notification_queue", autoAck: true, consumer: consumer);
            // await Task.CompletedTask;

            //     using (var scope = _scopeFactory.CreateScope())
            // {
            //     var adminRepo = scope.ServiceProvider.GetRequiredService<IAdminInterface>();
            //     var users =  adminRepo.GetAllUsers(); 

            //     foreach (var user in users)
            //     {
            //         string queueName = $"user_{user.c_UserId}_notifications";

            //         _channel.QueueDeclare(queue: queueName,
            //                             durable: true,
            //                             exclusive: false,
            //                             autoDelete: false,
            //                             arguments: null);

            //         var consumer = new EventingBasicConsumer(_channel);
            //         consumer.Received += async (model, ea) =>
            //         {
            //             var body = ea.Body.ToArray();
            //             var message = Encoding.UTF8.GetString(body);
            //             var notification = JsonConvert.DeserializeObject<t_Notification>(message);

            //             using var innerScope = _scopeFactory.CreateScope();
            //             var adminRepo = innerScope.ServiceProvider.GetRequiredService<IAdminInterface>();
            //             var redisService = innerScope.ServiceProvider.GetRequiredService<RedisService>();

            //             // Save notification to database
            //             await adminRepo.AddNotification(notification);

            //             // Cache and publish notification in Redis
            //             await redisService.CacheNotifications(notification.UserId, new List<t_Notification> { notification });
            //             redisService.PublishNotification(notification.UserId.ToString(), notification.Title);

            //             Console.WriteLine($"[RabbitMQ] Processed notification for user {notification.UserId}: {notification.Title}");
            //         };

            //         _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            //     }
            // }

            // await Task.CompletedTask;

            using var scope = _scopeFactory.CreateScope();
    var adminRepo = scope.ServiceProvider.GetRequiredService<IAdminInterface>();

    while (!stoppingToken.IsCancellationRequested)
    {
        var activeUsers = await adminRepo.GetUsersWithPendingNotifications();

        foreach (var userId in activeUsers)
        {
            string queueName = $"notifications_for_user_{userId}";

            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var notification = JsonConvert.DeserializeObject<t_Notification>(message);

                using var innerScope = _scopeFactory.CreateScope();
                var adminRepoInner = innerScope.ServiceProvider.GetRequiredService<IAdminInterface>();
                var MredisService = innerScope.ServiceProvider.GetRequiredService<MRedisService>();

                // await adminRepoInner.AddNotification(notification);
                await MredisService.CacheNotifications(notification.UserId, new List<t_Notification> { notification });
                MredisService.PublishNotification(notification.UserId.ToString(), notification.Title);

                Console.WriteLine($"[RabbitMQ] Processed notification for user {notification.UserId}: {notification.Title}");
            };

            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        }

        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Check every 10 seconds
    }
        }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();
        await base.StopAsync(cancellationToken);
    }
    }
}