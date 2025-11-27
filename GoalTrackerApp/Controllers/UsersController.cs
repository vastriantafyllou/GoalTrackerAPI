using Microsoft.AspNetCore.Mvc;
using GoalTrackerApp.Dto;
using GoalTrackerApp.Exceptions;
using GoalTrackerApp.Services;
using Microsoft.AspNetCore.Authorization;
using GoalTrackerApp.Core.Filters;
using GoalTrackerApp.Models;

namespace GoalTrackerApp.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : BaseController
    {
        private readonly IConfiguration _configuration;

        public UsersController(IApplicationService applicationService, IConfiguration configuration) : 
            base(applicationService)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<UserReadOnlyDto>> GetUserById(int id)
        {
            UserReadOnlyDto userReadOnlyDto = await ApplicationService.UserService.GetUserByIdAsync(id);
            return Ok(userReadOnlyDto);
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        [HttpGet("username/{username}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<UserReadOnlyDto>> GetUserByUsernameAsync(string? username)
        {
            var returnedUserDto = await ApplicationService.UserService.GetUserByUsernameAsync(username!);
            return Ok(returnedUserDto);
        }

        /// <summary>
        /// Get all users with pagination and filters
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
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
        /// Register a new user
        /// </summary>
        [HttpPost] 
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
        /// Update user by ID
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
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
        /// Delete user by ID
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await ApplicationService.UserService.DeleteUserAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Promote user to admin
        /// </summary>
        [HttpPatch("{id:int}/role/promote")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<UserReadOnlyDto>> PromoteToAdmin(int id)
        {
            var updateDto = new UserUpdateDto { UserRole = Core.Enums.UserRole.Admin };
            var updatedUser = await ApplicationService.UserService.UpdateUserAsync(id, updateDto);
            return Ok(updatedUser);
        }

        /// <summary>
        /// Demote admin to user
        /// </summary>
        [HttpPatch("{id:int}/role/demote")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<UserReadOnlyDto>> DemoteToUser(int id)
        {
            var updateDto = new UserUpdateDto { UserRole = Core.Enums.UserRole.User };
            var updatedUser = await ApplicationService.UserService.UpdateUserAsync(id, updateDto);
            return Ok(updatedUser);
        }
    }
}