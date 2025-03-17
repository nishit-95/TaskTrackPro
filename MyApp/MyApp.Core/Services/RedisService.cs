using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MyApp.Core.Model;
using MyApp.Core.Repositories.Interfaces;
using StackExchange.Redis;

namespace MyApp.Core.Services
{
    public class RedisService
    {
        private readonly IConnectionMultiplexer _redis;

        private readonly IUserInterface _userServices;
        public RedisService(IConnectionMultiplexer redis, IUserInterface userInterface)
        {
            _redis = redis;
            _userServices = userInterface;
        }

        public void SetTaskList(int userId, List<t_task_user> taskData)
        {
            var db = _redis.GetDatabase();
            string json = JsonSerializer.Serialize(taskData);
            db.StringSet($"TaskList:{userId}", json);
        }

        public async Task<List<t_task_user>> GetTaskList(int userId)
        {
            var db = _redis.GetDatabase();
            string json = await db.StringGetAsync($"TaskList:{userId}");
            if (json == null)
            {
                return new List<t_task_user>();
            }
            return JsonSerializer.Deserialize<List<t_task_user>>(json);
        }
    }
}