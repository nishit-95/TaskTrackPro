using StackExchange.Redis;

namespace MyApp.Core.Services
{
    public class RedisService
    {
        private readonly IConnectionMultiplexer _redis;

        public RedisService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public void SetAdminUnreadNotificationCount(int adminId, int count)
        {
            var db = _redis.GetDatabase();
            db.StringSet($"admin_unread_notifications:{adminId}", count);
        }

        public int GetAdminUnreadNotificationCount(int adminId)
        {
            var db = _redis.GetDatabase();
            var count = db.StringGet($"admin_unread_notifications:{adminId}");
            return count.HasValue ? (int)count : 0;
        }

        public void PublishAdminNotification(int adminId, string message)
        {
            var db = _redis.GetDatabase();
            db.Publish($"admin_notifications:{adminId}", message);
        }
    }
}