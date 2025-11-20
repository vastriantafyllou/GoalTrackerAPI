using GoalTrackerApp.Data;

namespace GoalTrackerApp.Repositories;

public interface IGoalCategoryRepository
{
    Task<GoalCategory?> GetAsync(int id);
    Task<IEnumerable<GoalCategory>> GetCategoriesByUserIdAsync(int userId);
}