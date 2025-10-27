using GoalTrackerApp.Data;
using GoalTrackerApp.Dto;

namespace GoalTrackerApp.Repositories
{
    public interface IGoalRepository 
    {
        Task<IEnumerable<Goal>> GetGoalsByUserIdAsync(int userId);
        Task<int> GetTotalGoalsCountByUserIdAsync(int userId);
        Task<int> GetInProgressGoalsCountByUserIdAsync(int userId);
        Task<int> GetCompletedGoalsCountByUserIdAsync(int userId);
        Task<IEnumerable<CategoryGoalCountDto>> GetGoalCountPerCategoryAsync(int userId);
            
        
    }
}