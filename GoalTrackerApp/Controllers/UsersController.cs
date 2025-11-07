using Microsoft.AspNetCore.Mvc;
using GoalTrackerApp.Dto;
using GoalTrackerApp.Exceptions;
using GoalTrackerApp.Services;
using Microsoft.AspNetCore.Authorization;
using GoalTrackerApp.Core.Filters;
using GoalTrackerApp.Models;

namespace GoalTrackerApp.Controllers
{
    public class UsersController : BaseController
    {
        private readonly IConfiguration _configuration;

        public UsersController(IApplicationService applicationService, IConfiguration configuration) : 
            base(applicationService)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Gets a user by ID. Admin only.
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserReadOnlyDto>> GetUserById(int id)
        {
            UserReadOnlyDto userReadOnlyDto = await ApplicationService.UserService.GetUserByIdAsync(id);
            return Ok(userReadOnlyDto);
        }

        /// <summary>
        /// Gets a user by username. Admin only.
        /// </summary>
        [HttpGet("by-username/{username}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserReadOnlyDto>> GetUserByUsernameAsync(string? username)
        {
            var returnedUserDto = await ApplicationService.UserService.GetUserByUsernameAsync(username!);
            return Ok(returnedUserDto);
        }

        /// <summary>
        /// Gets all users with pagination and filters. Admin only.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaginatedResult<UserReadOnlyDto>>> GetAllUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? username = null,
            [FromQuery] string? email = null,
            [FromQuery] string? userRole = null)
        {
            var filters = new UserFiltersDto
            {
                Username = username,
                Email = email,
                UserRole = userRole
            };

            var result = await ApplicationService.UserService.GetPaginatedUsersFilteredAsync(
                pageNumber, pageSize, filters);
            
            return Ok(result);
        }
        
        /// <summary>
        /// Registers a new user in the system
        /// </summary>
        /// <param name="userSignupDto">The user registration details</param>
        /// <returns>The newly created user's information</returns>
        /// <response code="201">User successfully registered</response>
        /// <response code="400">Invalid input data</response>
        /// <exception cref="InvalidRegistrationException">Thrown when registration data is invalid</exception>
        [HttpPost("register")] 
        public async Task<ActionResult<UserReadOnlyDto>> RegisterUserAsync([FromBody] UserSignupDto userSignupDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(e => e.Value!.Errors.Any())
                    .Select(e => new { 
                        Field = e.Key, 
                        Errors = e.Value!.Errors.Select(er => er.ErrorMessage).ToArray()
                    });
                throw new InvalidRegistrationException("ErrorsInRegistration" + errors);
            }

            UserReadOnlyDto returnedUserDto = await ApplicationService.UserService.SignUpUserAsync(userSignupDto);
            return StatusCode(201, returnedUserDto);
        }

        /// <summary>
        /// Updates a user. Admin only.
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserReadOnlyDto>> UpdateUser(int id, [FromBody] UserUpdateDto userUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(e => e.Value!.Errors.Any())
                    .Select(e => new { 
                        Field = e.Key, 
                        Errors = e.Value!.Errors.Select(er => er.ErrorMessage).ToArray()
                    });
                throw new InvalidArgumentException("User", "Invalid update data: " + errors);
            }

            var updatedUser = await ApplicationService.UserService.UpdateUserAsync(id, userUpdateDto);
            return Ok(updatedUser);
        }

        /// <summary>
        /// Deletes a user (soft delete). Admin only.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await ApplicationService.UserService.DeleteUserAsync(id);
            return NoContent();
        }
    }
}