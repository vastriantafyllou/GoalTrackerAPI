using Microsoft.AspNetCore.Mvc;
using GoalTrackerApp.Dto;
using GoalTrackerApp.Services;

namespace GoalTrackerApp.Controllers;

/// <summary>
/// Handles user-related operations (read-only).
/// For authentication operations, see AuthController.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : BaseController
{
    public UsersController(IApplicationService applicationService) 
        : base(applicationService)
    {
    }

    /// <summary>
    /// Gets a user by their ID.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>User information</returns>
    /// <response code="200">Returns the user</response>
    /// <response code="404">User not found</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserReadOnlyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserReadOnlyDto>> GetUserByIdAsync(int id)
    {
        var userReadOnlyDto = await ApplicationService.UserService.GetUserByIdAsync(id);
        return Ok(userReadOnlyDto);
    }

    /// <summary>
    /// Gets a user by their username.
    /// </summary>
    /// <param name="username">The username to search for</param>
    /// <returns>User information</returns>
    /// <response code="200">Returns the user</response>
    /// <response code="400">Username is required</response>
    /// <response code="404">User not found</response>
    [HttpGet("username/{username}")]
    [ProducesResponseType(typeof(UserReadOnlyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserReadOnlyDto>> GetUserByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest("Username cannot be empty.");
        }

        var returnedUserDto = await ApplicationService.UserService.GetUserByUsernameAsync(username);
        return Ok(returnedUserDto);
    }
}