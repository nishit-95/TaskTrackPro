using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyApp.Core.Models;
using MyApp.Core.Repositories.Interfaces;

namespace MyApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfileApiController : ControllerBase
    {
        private readonly IUserProfileInterface _userProfileRepo;
        public UserProfileApiController(IUserProfileInterface userProfileRepo)
        {
            _userProfileRepo = userProfileRepo;
        }

        [HttpGet]
        [Route("getuserdata")]
        public async Task<IActionResult> Get(string email)
        {
            try
            {
                var user = await _userProfileRepo.GetOneUser(email);
                if (user == null)
                {
                    return NotFound("User profile not found");
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving user profile: {ex.Message}");
                // _logger.LogError($"An error occurred while retrieving user profile: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPut("update-profile")]
        public async Task<IActionResult> Put([FromForm] vm_UserProfile user)
        {
            try
            {        
                var existingUser =await _userProfileRepo.GetOneUser("user@example.com");
                // Console.WriteLine("user found -> "+ existingUser.ToString());                                
                // var existingUser = _userProfileRepository.GetOneUser(user.c_email);
                if (existingUser == null)
                {
                    return NotFound("User Profile not found");
                }
                // if (photo != null)
                // {
                //     var uploadsFolder = Path.Combine("..\\MVC\\wwwroot", "photo");
                //     string uniqueFilename = Guid.NewGuid().ToString() + "_" + photo.FileName;
                //     string filepath = Path.Combine(uploadsFolder, uniqueFilename);

                //     // Save file asynchronously
                //     await using var stream = new FileStream(filepath, FileMode.Create);
                //     await photo.CopyToAsync(stream);

                //     user.c_Image = uniqueFilename;
                // }
                // else
                // {
                //     user.c_Image = existingUser.c_Image;
                // }
                Console.WriteLine("While updating the user -> " + user.c_Address);
                await _userProfileRepo.Update(user);

                return Ok("User profile updated successfully");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating user profile: {ex.Message}");
                return StatusCode(500, "Internal server error");    
                // return null;
            }
        }

    }
}