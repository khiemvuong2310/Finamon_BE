using Finamon.Service.Interfaces;
using Finamon.Service.ReponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finamon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserActivityController : ControllerBase
    {
        private readonly IUserActivityService _userActivityService;

        public UserActivityController(IUserActivityService userActivityService)
        {
            _userActivityService = userActivityService;
        }

        /// <summary>
        /// Log user activity when they login
        /// </summary>
        [HttpPost("log/{userId}")]
        public async Task<IActionResult> LogUserActivity(int userId , int useTimeInMinutes)
        {
            try
            {
                await _userActivityService.LogUserActivityAsync(userId, useTimeInMinutes);
                return Ok(new { message = "User activity logged successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get user activities by user id
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserActivityResponse>>> GetUserActivities(int userId)
        {
            try
            {
                var activities = await _userActivityService.GetUserActivitiesAsync(userId);
                return Ok(activities);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get user activity statistics
        /// </summary>
        [HttpGet("stats/{userId}")]
        public async Task<ActionResult<UserActivityStatsResponse>> GetUserActivityStats(int userId)
        {
            try
            {
                var stats = await _userActivityService.GetUserActivityStatsAsync(userId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
} 