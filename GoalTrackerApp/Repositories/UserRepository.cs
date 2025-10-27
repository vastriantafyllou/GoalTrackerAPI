using System.Linq.Expressions;
using GoalTrackerApp.Data;
using GoalTrackerApp.Models;
using GoalTrackerApp.Security;
using Microsoft.EntityFrameworkCore;

namespace GoalTrackerApp.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(GoalTrackerAppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetUserAsync(string username, string password)
    {
        // Allow login using either username or email
        var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username 
                                                                || u.Email == username);
        
        if (user == null) return null;
        if (!EncryptionUtil.IsValidPassword(password,  user.Password)) return null;
        return user;
    }

    public async Task<User?> GetUserByUsernameAsync(string username) =>
        await context.Users.FirstOrDefaultAsync(u => u.Username == username);
    

    public async Task<PaginatedResult<User>> GetUsersAsync(int pageNumber, int pageSize, List<Expression<Func<User, bool>>> predicates)
    {
        // IQueryable builds the query, it does not execute it yet.
        IQueryable<User> query = context.Users; 

        if (predicates != null && predicates.Count > 0)
        {
            foreach (var predicate in predicates)
            {
                query = query.Where(predicate);
            }
        }

        int totalRecords = await query.CountAsync(); // Query executes here to get the count
        
        int skip = (pageNumber - 1) * pageSize;
        
        var data = await query
            .OrderBy(u => u.Id) // Required for stable pagination
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(); // Query executes here to get the paginated data
            
        return new PaginatedResult<User>(data, totalRecords, pageNumber, pageSize);
    }
}