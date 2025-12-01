using GoalTrackerApp.Core.Enums;
using GoalTrackerApp.Core.Filters;
using GoalTrackerApp.Data;
using GoalTrackerApp.Dto;
using GoalTrackerApp.Models;

namespace GoalTrackerApp.Services
{
    public interface IUserService
    {
        Task<User?> VerifyAndGetUserAsync(UserLoginDto credentials);
        Task<UserReadOnlyDto?> GetUserByUsernameAsync(string username);
        Task<UserReadOnlyDto> GetUserByIdAsync(int id);
        Task<PaginatedResult<UserReadOnlyDto>> GetPaginatedUsersFilteredAsync(int pageNumber, 
            int pageSize, UserFiltersDto userFiltersDto);
        Task<UserReadOnlyDto> SignUpUserAsync(UserSignupDto signupDto);
        Task<UserReadOnlyDto> UpdateUserAsync(int userId, UserUpdateDto updateDto);
        Task DeleteUserAsync(int userId);
        string CreateUserToken(int userId, string username, string email, UserRole userRole, string appSecurityKey);
        Task SendPasswordRecoveryEmailAsync(string email);
        Task ResetPasswordAsync(PasswordResetDto resetDto);
        Task<PasswordResetTokenValidationDto> ValidateResetTokenAsync(string token);
    }
}