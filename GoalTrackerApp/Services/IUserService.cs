using GoalTrackerApp.Core.Enums;
using GoalTrackerApp.Core.Filters;
using GoalTrackerApp.Data;
using GoalTrackerApp.Dto;
using GoalTrackerApp.Models;

namespace GoalTrackerApp.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Verifies user credentials and returns the user entity.
        /// </summary>
        Task<User?> VerifyAndGetUserAsync(UserLoginDto credentials);
        
        /// <summary>
        /// Retrieves a user by their username.
        /// </summary>
        Task<UserReadOnlyDto?> GetUserByUsernameAsync(string username);
        
        /// <summary>
        /// Retrieves a user by their ID.
        /// </summary>
        Task<UserReadOnlyDto> GetUserByIdAsync(int id);
        
        /// <summary>
        /// Gets paginated users with optional filters.
        /// </summary>
        Task<PaginatedResult<UserReadOnlyDto>> GetPaginatedUsersFilteredAsync(int pageNumber, 
            int pageSize, UserFiltersDto userFiltersDto);
        
        /// <summary>
        /// Registers a new user.
        /// </summary>
        Task<UserReadOnlyDto> SignUpUserAsync(UserSignupDto signupDto);
        
        /// <summary>
        /// Creates a JWT token for the authenticated user.
        /// </summary>
        string CreateUserToken(int userId, string username, string email, UserRole userRole, 
            int expirationHours, string appSecurityKey, string issuer, string audience);
    }
}