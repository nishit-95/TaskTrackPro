using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;
using System.Threading.Tasks;
using MyApp.Core.Models;
using Newtonsoft.Json;

namespace MyApp.Core.Services
{
    public class MRedisService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly ISubscriber _subscriber;

        public MRedisService(string connectionString)
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _db = _redis.GetDatabase();
            _subscriber = _redis.GetSubscriber();
        }

        // public void PublishMessage(string channel, string message)
        // {
        //     var subscriber = _redis.GetSubscriber();
        //     subscriber.Publish(channel, message);
        // }

        // public void Subscribe(string channel, Action<RedisChannel, RedisValue> handler)
        // {
        //     var subscriber = _redis.GetSubscriber();
        //     subscriber.Subscribe(channel, handler);
        // }

        public async Task PublishNotification(string userId, string message)
        {
            // await _subscriber.PublishAsync($"notifications:{userId}", message);
            string channel = $"notifications:{userId}";
            await _subscriber.PublishAsync(channel, message);
        }

        public void SubscribeNotifications(int userId, Action<string> handler)
        {
            // _subscriber.Subscribe($"notifications:{userId}", (channel, message) =>
            // {
            //     handler(message);
            // });
            string channel = $"notifications:{userId}";
            _subscriber.Subscribe(channel, (channel, message) =>
            {
                handler(message);
            });
        }
        public async Task CacheNotifications(int userId, List<t_Notification> newNotifications)
        {
            string key = $"notifications:{userId}";

             var existingData = await _db.StringGetAsync(key);
            List<t_Notification> notifications = !existingData.IsNullOrEmpty ? JsonConvert.DeserializeObject<List<t_Notification>>(existingData) : new List<t_Notification>();

            var uniqueNewNotifications = newNotifications
        .Where(newNotif => !notifications.Any(existingNotif => existingNotif.NotificationId == newNotif.NotificationId))
        .ToList();

            notifications.AddRange(newNotifications);

            string serializedData = JsonConvert.SerializeObject(notifications);
            await _db.StringSetAsync(key, serializedData, TimeSpan.FromHours(24));
        }

        public async Task RemoveNotificationFromRedis(int userId, int notificationId)
        {
            string key = $"notifications:{userId}";

            // Fetch the current notifications from Redis
            var existingData = await _db.StringGetAsync(key);
            if (existingData.IsNullOrEmpty)
            {
                return; // No data in Redis
            }

            // Deserialize the notifications
            var notifications = JsonConvert.DeserializeObject<List<t_Notification>>(existingData);

            // Remove the notification with the specified NotificationId
            notifications.RemoveAll(n => n.NotificationId == notificationId);

            // Save the updated list back to Redis
            string serializedData = JsonConvert.SerializeObject(notifications);
            await _db.StringSetAsync(key, serializedData, TimeSpan.FromHours(24));
        }

        public async Task<List<t_Notification>> GetCachedNotifications(int userId)
        {
            string key = $"notifications:{userId}";
            var data = await _db.StringGetAsync(key);
            return !data.IsNullOrEmpty ? JsonConvert.DeserializeObject<List<t_Notification>>(data) : new List<t_Notification>();
        }
    }
}