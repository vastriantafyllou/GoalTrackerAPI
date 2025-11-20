using GoalTrackerApp.Data;
using Microsoft.EntityFrameworkCore;

namespace GoalTrackerApp.Repositories;

public class GoalCategoryRepository : BaseRepository<GoalCategory>, IGoalCategoryRepository
{
    public GoalCategoryRepository(GoalTrackerAppDbContext context) : base(context)
    {
    }
    
    public override async Task<GoalCategory?> GetAsync(int id)
    {
        return await dbSet
            .Include(c => c.Goals)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    public async Task<IEnumerable<GoalCategory>> GetCategoriesByUserIdAsync(int userId)
    {
        return await dbSet
            .Include(c => c.Goals)
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .ToListAsync();
    }
}