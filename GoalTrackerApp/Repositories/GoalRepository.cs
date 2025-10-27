using GoalTrackerApp.Core.Enums;
using GoalTrackerApp.Data;
using GoalTrackerApp.Dto;
using Microsoft.EntityFrameworkCore;

namespace GoalTrackerApp.Repositories;

public class GoalRepository : BaseRepository<Goal>, IGoalRepository
{
    public GoalRepository(GoalTrackerAppDbContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<Goal>> GetGoalsByUserIdAsync(int userId)
    {
        return await dbSet
            .Where(g => g.UserId == userId)
            .ToListAsync();
    }
    
    public async Task<int> GetTotalGoalsCountByUserIdAsync(int userId)
    {
        return await dbSet
            .Where(goal => goal.UserId == userId)
            .CountAsync();
    }
    
    public async Task<int> GetInProgressGoalsCountByUserIdAsync(int userId)
    {
        return await dbSet
            .Where(g => g.UserId == userId && g.GoalStatus == GoalStatus.InProgress)
            .CountAsync();
    }
    
    public async Task<int> GetCompletedGoalsCountByUserIdAsync(int userId)
    {
        return await dbSet
            .Where(g => g.UserId == userId && g.GoalStatus == GoalStatus.Completed)
            .CountAsync();
    }
    
    // Groups goals by category name and returns a count for each.
    public async Task<IEnumerable<CategoryGoalCountDto>> GetGoalCountPerCategoryAsync(int userId)
    {
        return await dbSet
            .Where(g => g.UserId == userId)
            .Include(g => g.GoalCategory)
            // Handle goals that might not have a category (null)
            .GroupBy(g => g.GoalCategory == null ? "Uncategorized" : g.GoalCategory.Name)
            .Select(group => new CategoryGoalCountDto
            {
                CategoryName = group.Key,
                GoalCount = group.Count()
            })
            .ToListAsync();
    }
    
    
    
    
}