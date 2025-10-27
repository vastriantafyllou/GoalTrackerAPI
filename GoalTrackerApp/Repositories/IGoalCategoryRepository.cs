using GoalTrackerApp.Data;

namespace GoalTrackerApp.Repositories;

public interface IGoalCategoryRepository
{
    Task<IEnumerable<GoalCategory>> GetCategoriesByUserIdAsync(int userId);
}