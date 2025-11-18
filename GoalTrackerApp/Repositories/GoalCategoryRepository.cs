using GoalTrackerApp.Data;
using Microsoft.EntityFrameworkCore;

namespace GoalTrackerApp.Repositories;

public class GoalCategoryRepository : BaseRepository<GoalCategory>, IGoalCategoryRepository
{
    public GoalCategoryRepository(GoalTrackerAppDbContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<GoalCategory>> GetCategoriesByUserIdAsync(int userId)
    {
        return await dbSet
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .ToListAsync();
    }
}