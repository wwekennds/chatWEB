using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Chat.Services;
using Chat.Hubs;


namespace Chat.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersChatController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersChatController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{roomName}/users")]
        public async Task<IActionResult> GetRoomUsers(string roomName)
        {
            try
            {
                Console.WriteLine($"GetRoomUsers called for room: {roomName}");
                var userList = await _userService.GetUsersFromRoom(roomName);
                Console.WriteLine($"Returning {userList.UserInRoom?.Count} users");
                return Ok(userList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetRoomUsers: {ex.Message}");
                return BadRequest($"Error getting users: {ex.Message}");
            }
        }
    }
}