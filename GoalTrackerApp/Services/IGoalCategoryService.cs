using GoalTrackerApp.Dto;

namespace GoalTrackerApp.Services
{
    public interface IGoalCategoryService
    {
        /// <summary>
        /// Creates a new category for a specific user.
        /// </summary>
        /// <param name="dto">The data for the new category.</param>
        /// <param name="userId">The ID of the user creating the category.</param>
        /// <returns>The newly created category.</returns>
        Task<GoalCategoryReadOnlyDto> CreateCategoryAsync(GoalCategoryCreateDto dto, int userId);

        /// <summary>
        /// Returns all categories belonging to a specific user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A list of the user's categories.</returns>
        Task<IEnumerable<GoalCategoryReadOnlyDto>> GetCategoriesForUserAsync(int userId);

        /// <summary>
        /// Returns a specific category after verifying it belongs to the user.
        /// </summary>
        /// <param name="categoryId">The ID of the category to retrieve.</param>
        /// <param name="userId">The ID of the user making the request.</param>
        /// <returns>The category.</returns>
        Task<GoalCategoryReadOnlyDto> GetCategoryByIdAsync(int categoryId, int userId);

        /// <summary>
        /// Updates an existing category after verifying it belongs to the user.
        /// </summary>
        /// <param name="categoryId">The ID of the category to update.</param>
        /// <param name="dto">The new category data.</param>
        /// <param name="userId">The ID of the user making the request.</param>
        Task UpdateCategoryAsync(int categoryId, GoalCategoryUpdateDto dto, int userId);

        /// <summary>
        /// Deletes a category after verifying it belongs to the user.
        /// </summary>
        /// <param name="categoryId">The ID of the category to delete.</param>
        /// <param name="userId">The ID of the user making the request.</param>
        Task DeleteCategoryAsync(int categoryId, int userId);
    }
}
