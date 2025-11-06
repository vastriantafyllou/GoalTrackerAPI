using GoalTrackerApp.Dto;

namespace GoalTrackerApp.Services
{
    public interface IGoalService
    {
        /// <summary>
        /// Creates a new goal for a specific user.
        /// </summary>
        /// <param name="dto">The data for the new goal.</param>
        /// <param name="userId">The ID of the user creating the goal.</param>
        /// <returns>The new created goal.</returns>
        Task<GoalReadOnlyDto> CreateGoalAsync(GoalCreateDto dto, int userId);

        /// <summary>
        /// Returns all goals belonging to a specific user.
        /// </summary>
        /// <param name="userId">The user's ID.</param>
        /// <returns>A list of the user's goals.</returns>
        Task<IEnumerable<GoalReadOnlyDto>> GetGoalsForUserAsync(int userId);

        /// <summary>
        /// Returns a specific goal,
        /// after first checking if it belongs to the user.
        /// </summary>
        /// <param name="goalId">The ID of the goal to retrieve.</param>
        /// <param name="userId">The ID of the user making the request.</param>
        /// <returns>The goal.</returns>
        Task<GoalReadOnlyDto> GetGoalByIdAsync(int goalId, int userId);

        /// <summary>
        /// Updates an existing goal,
        /// after first checking if it belongs to the user.
        /// </summary>
        /// <param name="goalId">The ID of the goal to update.</param>
        /// <param name="dto">The new data for the goal.</param>
        /// <param name="userId">The ID of the user making the request.</param>
        Task UpdateGoalAsync(int goalId, GoalUpdateDto dto, int userId);

        /// <summary>
        /// Deletes a goal,
        /// after first checking if it belongs to the user.
        /// </summary>
        /// <param name="goalId">The ID of the goal to delete.</param>
        /// <param name="userId">The ID of the user making the request.</param>
        Task DeleteGoalAsync(int goalId, int userId);
    }
}