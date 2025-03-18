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
    public interface IRedisService
    {
        Task Set(string key, string value);
        Task<string> Get(string key);
    }

    public class RedisService : IRedisService
    {
        private readonly IDatabase _database;

        public RedisService(IDatabase database)
        {
            _database = database;
        }

        public async Task<string> Get(string key)
        {
            return await _database.StringGetAsync(key);
        }

        public async Task Set(string key, string value)
        {
            await _database.StringSetAsync(key, value);
        }


        public void SetTaskList(int userId, List<t_task_user> taskData)
        {
            string json = JsonSerializer.Serialize(taskData);
            _database.StringSet($"TaskList:{userId}", json);
        }

        public async Task<List<t_task_user>> GetTaskList(int userId)
        {
            string json = await _database.StringGetAsync($"TaskList:{userId}");
            if (json == null)
            {
                return new List<t_task_user>();
            }
            return JsonSerializer.Deserialize<List<t_task_user>>(json);
        }

    }
}