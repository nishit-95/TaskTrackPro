using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyApp.Core.Model;
using MyApp.Core.Repositories.Interfaces;
using MyApp.Core.Models;
using MyApp.Core.Services;

namespace MyApp.API.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class UserApiController : ControllerBase
    {
        private readonly IUserInterface _userServices;
        private readonly ElasticSearchService _elasticSearchService;
        private readonly RabbitMQService _rabbitMQService;

        public UserApiController(ElasticSearchService elasticSearchService, RabbitMQService rabbitMQService, IUserInterface userServices)
        {
            _elasticSearchService = elasticSearchService;
            _rabbitMQService = rabbitMQService;
            _userServices = userServices;
        }

        [HttpGet]
        [Route("GetTaskByUserId/{userId}")]
        public async Task<IActionResult> GetTaskByUserId(int userId)
        {
            List<t_task_user> TaskList = await _userServices.GetTaskByUserId(userId);
            if (TaskList == null)
            {
                return BadRequest("From User API Controller : There was some error while fetching the tasks");
            }
            return Ok(TaskList);
        }

        [HttpGet]
        [Route("UpdateStatus/{taskId}")]
        public async Task<IActionResult> UpdateStatus(int taskId)
        {
            var status = await _userServices.UpdateStatus(taskId);
            if (status == 0)
            {
                return BadRequest("From User API Controller : Status not updated");
            }
            else if (status == 1)
            {
                return Ok("Status Updated Successfully");
            }
            else
            {
                return BadRequest("From User API Controller : There was some error while updating the status");
            }
        }

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