using GoalTrackerApp.Dto;
using GoalTrackerApp.Exceptions;
using GoalTrackerApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoalTrackerApp.Controllers;

    /// <summary>
    /// Controller for managing Goals.
    /// [Authorize] locks the entire controller - all endpoints require authentication.
    /// </summary>
    [Authorize]
    // [Route("api/[controller]")]
    // [ApiController]
    public class GoalsController : BaseController
    {
        public GoalsController(IApplicationService applicationService) 
            : base(applicationService)
        {
        }

        /// <summary>
        /// Creates a new goal for the authenticated user.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<GoalReadOnlyDto>> CreateGoal([FromBody] GoalCreateDto dto)
        {
            var userId = GetCurrentUserId();
            var newGoal = await ApplicationService.GoalService.CreateGoalAsync(dto, userId);
            
            return CreatedAtAction(nameof(GetGoal), new { id = newGoal.Id }, newGoal);
        }

        /// <summary>
        /// Returns all goals for the authenticated user.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GoalReadOnlyDto>>> GetMyGoals()
        {
            var userId = GetCurrentUserId();
            var goals = await ApplicationService.GoalService.GetGoalsForUserAsync(userId);
            return Ok(goals);
        }

        /// <summary>
        /// Returns a specific goal if it belongs to the authenticated user.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<GoalReadOnlyDto>> GetGoal(int id)
        {
            var userId = GetCurrentUserId();
            var goal = await ApplicationService.GoalService.GetGoalByIdAsync(id, userId);
            return Ok(goal);
        }

        /// <summary>
        /// Updates a goal if it belongs to the authenticated user.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGoal(int id, [FromBody] GoalUpdateDto dto)
        {
            var userId = GetCurrentUserId();
            await ApplicationService.GoalService.UpdateGoalAsync(id, dto, userId);
            
            return NoContent();
        }

        /// <summary>
        /// Deletes a goal if it belongs to the authenticated user.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGoal(int id)
        {
            var userId = GetCurrentUserId();
            await ApplicationService.GoalService.DeleteGoalAsync(id, userId);
            
            return NoContent();
        }

        /// <summary>
        /// Extracts the current user's ID from the JWT token claims.
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                throw new EntityNotAuthorizedException("User", "User ID claim not found in token.");
            }
            return int.Parse(userIdString);
        }
    }
