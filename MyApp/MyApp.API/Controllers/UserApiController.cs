using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyApp.Core.Model;
using MyApp.Core.Repositories.Interfaces;
using MyApp.Core.Models;
using MyApp.Core.Services;
using Elastic.Clients.Elasticsearch.MachineLearning;

namespace MyApp.API.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class UserApiController : ControllerBase
    {
        private readonly IUserInterface _userServices;
        private readonly ElasticSearchService _elasticSearchService;
        // private readonly RabbitMQService _rabbitMQService;
        private readonly RedisService _redisService;

        public UserApiController(ElasticSearchService elasticSearchService, IUserInterface userServices, RedisService redisService)
        {
            // RabbitMQService rabbitMQService
            _elasticSearchService = elasticSearchService;
            // _rabbitMQService = rabbitMQService;
            _userServices = userServices;
            _redisService = redisService;
        }

        [HttpGet]
        [Route("GetTaskByUserId/{userId}")]


        #region task actions
        public async Task<IActionResult> GetTaskByUserId(int userId)
        {
            List<t_task_user> taskList = await _redisService.GetTaskList(userId);
            System.Console.WriteLine("from redis");
            if (taskList.Count == 0)
            {
                System.Console.WriteLine("form database");
                taskList = await _userServices.GetTaskByUserId(userId);
                if (taskList == null)
                {
                    return BadRequest("From User API Controller : There was some error while fetching the tasks");
                }
                _redisService.SetTaskList(userId, taskList);
            }
            return Ok(taskList);
        }

        [HttpGet]
        [Route("UpdateStatus/{taskId}/{userId}")]
        public async Task<IActionResult> UpdateStatus(int taskId, int userId)
        {
            System.Console.WriteLine(userId);
            var status = await _userServices.UpdateStatus(taskId);
            if (status == 0)
            {
                return BadRequest("From User API Controller : Status not updated");
            }
            else if (status == 1)
            {
                List<t_task_user> taskList = await _userServices.GetTaskByUserId(userId);
                _redisService.SetTaskList(userId, taskList);
                return Ok("Status Updated Successfully");
            }
            else
            {
                return BadRequest("From User API Controller : There was some error while updating the status");
            }
        }

        #endregion

        [HttpPost("index")]
        public async Task<IActionResult> IndexTask([FromBody] t_task task)
        {
            await _elasticSearchService.IndexTask(task);
            return Ok("Task indexed successfully.");
        }

        // ✅ Search & Filter Tasks
        [HttpGet("search")]
        public async Task<IActionResult> SearchTasks([FromQuery] string query, [FromQuery] string status = null)
        {
            var results = await _elasticSearchService.SearchTasks(query, status);
            return Ok(results);
        }
    }
}

