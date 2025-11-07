using System.Linq.Expressions;
using GoalTrackerApp.Core.Security;
using GoalTrackerApp.Data;
using GoalTrackerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GoalTrackerApp.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(GoalTrackerAppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetUserAsync(string username, string password)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username 
                                                                || u.Email == username);
        
        if (user == null) return null;
        if (!EncryptionUtil.IsValidPassword(password,  user.Password)) return null;
        return user;
    }

    public async Task<User?> GetUserByUsernameAsync(string username) =>
        await context.Users.FirstOrDefaultAsync(u => u.Username == username);
    
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<PaginatedResult<User>> GetUsersAsync(int pageNumber, int pageSize, List<Expression<Func<User, bool>>> predicates)
    {
        IQueryable<User> query = context.Users; 

        if (predicates != null && predicates.Count > 0)
        {
            foreach (var predicate in predicates)
            {
                query = query.Where(predicate);
            }
        }

        int totalRecords = await query.CountAsync();
        
        int skip = (pageNumber - 1) * pageSize;
        
        var data = await query
            .OrderBy(u => u.Id)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
            
        return new PaginatedResult<User>(data, totalRecords, pageNumber, pageSize);
    }
}