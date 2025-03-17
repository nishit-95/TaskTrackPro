using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyApp.Core.Models;
using MyApp.Core.Services;

namespace MyApp.API.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class UserApiController : ControllerBase
    {
        private readonly ElasticSearchService _elasticSearchService;
        private readonly RabbitMQService _rabbitMQService;

        public UserApiController(ElasticSearchService elasticSearchService, RabbitMQService rabbitMQService)
        {
            _elasticSearchService = elasticSearchService;
            _rabbitMQService = rabbitMQService;
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