using GoalTrackerApp.Dto;
using GoalTrackerApp.Exceptions;
using GoalTrackerApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoalTrackerApp.Controllers;

/// <summary>
/// Manages user goals (CRUD operations).
/// All endpoints require authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GoalsController : BaseController
{
    public GoalsController(IApplicationService applicationService) 
        : base(applicationService)
    {
    }

    /// <summary>
    /// Creates a new goal for the authenticated user.
    /// </summary>
    /// <param name="dto">Goal creation data</param>
    /// <returns>The newly created goal</returns>
    /// <response code="201">Goal successfully created</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost]
    [ProducesResponseType(typeof(GoalReadOnlyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GoalReadOnlyDto>> CreateGoal([FromBody] GoalCreateDto dto)
    {
        var userId = GetCurrentUserId();
        var newGoal = await ApplicationService.GoalService.CreateGoalAsync(dto, userId);
        
        return CreatedAtAction(nameof(GetGoal), new { id = newGoal.Id }, newGoal);
    }

    /// <summary>
    /// Gets all goals for the authenticated user.
    /// </summary>
    /// <returns>List of user's goals</returns>
    /// <response code="200">Returns the list of goals</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GoalReadOnlyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<GoalReadOnlyDto>>> GetMyGoals()
    {
        var userId = GetCurrentUserId();
        var goals = await ApplicationService.GoalService.GetGoalsForUserAsync(userId);
        return Ok(goals);
    }

    /// <summary>
    /// Gets a specific goal by ID. User must own the goal.
    /// </summary>
    /// <param name="id">The goal ID</param>
    /// <returns>The requested goal</returns>
    /// <response code="200">Returns the goal</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Goal not found or user doesn't have access</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(GoalReadOnlyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GoalReadOnlyDto>> GetGoal(int id)
    {
        var userId = GetCurrentUserId();
        var goal = await ApplicationService.GoalService.GetGoalByIdAsync(id, userId);
        return Ok(goal);
    }

    /// <summary>
    /// Updates an existing goal. User must own the goal.
    /// </summary>
    /// <param name="id">The goal ID to update</param>
    /// <param name="dto">Updated goal data</param>
    /// <returns>No content</returns>
    /// <response code="204">Goal successfully updated</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Goal not found or user doesn't have access</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGoal(int id, [FromBody] GoalUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        await ApplicationService.GoalService.UpdateGoalAsync(id, dto, userId);
        
        return NoContent();
    }

    /// <summary>
    /// Deletes a goal. User must own the goal.
    /// </summary>
    /// <param name="id">The goal ID to delete</param>
    /// <returns>No content</returns>
    /// <response code="204">Goal successfully deleted</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Goal not found or user doesn't have access</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteGoal(int id)
    {
        var userId = GetCurrentUserId();
        await ApplicationService.GoalService.DeleteGoalAsync(id, userId);
        
        return NoContent();
    }

    /// <summary>
    /// Extracts the authenticated user's ID from JWT claims.
    /// </summary>
    /// <returns>The current user's ID</returns>
    /// <exception cref="EntityNotAuthorizedException">Thrown when user ID claim is missing</exception>
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new EntityNotAuthorizedException("User", "User ID claim not found or invalid in token.");
        }
        
        return userId;
    }
}
