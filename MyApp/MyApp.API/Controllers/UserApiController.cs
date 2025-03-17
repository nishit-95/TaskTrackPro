using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyApp.Core.Model;
using MyApp.Core.Repositories.Interfaces;

namespace MyApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserApiController : ControllerBase
    {
        private readonly IUserInterface _userServices;

        public UserApiController(IUserInterface userServices)
        {
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
    }
}