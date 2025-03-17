using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    }
}