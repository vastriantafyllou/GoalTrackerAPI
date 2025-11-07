using System.Linq.Expressions;
using GoalTrackerApp.Data;
using GoalTrackerApp.Models;

namespace GoalTrackerApp.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserAsync(string username, string password);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task<PaginatedResult<User>> GetUsersAsync(int pageNumber, int pageSize,
        List<Expression<Func<User, bool>>> predicates);
}