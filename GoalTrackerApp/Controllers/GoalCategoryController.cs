using GoalTrackerApp.Dto;
using GoalTrackerApp.Exceptions;
using GoalTrackerApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoalTrackerApp.Controllers
{
    /// <summary>
    /// Controller for managing GoalCategories.
    /// All endpoints require authentication.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GoalCategoryController : BaseController
    {
        public GoalCategoryController(IApplicationService applicationService) 
            : base(applicationService)
        {
        }

        /// <summary>
        /// Creates a new category for the authenticated user.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<GoalCategoryReadOnlyDto>> CreateCategory([FromBody] GoalCategoryCreateDto dto)
        {
            var userId = GetCurrentUserId();
            var newCategory = await ApplicationService.GoalCategoryService.CreateCategoryAsync(dto, userId);
            
            return CreatedAtAction(nameof(GetCategory), new { id = newCategory.Id }, newCategory);
        }

        /// <summary>
        /// Returns all categories for the authenticated user.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GoalCategoryReadOnlyDto>>> GetMyCategories()
        {
            var userId = GetCurrentUserId();
            var categories = await ApplicationService.GoalCategoryService.GetCategoriesForUserAsync(userId);
            return Ok(categories);
        }

        /// <summary>
        /// Returns a specific category if it belongs to the authenticated user.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<GoalCategoryReadOnlyDto>> GetCategory(int id)
        {
            var userId = GetCurrentUserId();
            var category = await ApplicationService.GoalCategoryService.GetCategoryByIdAsync(id, userId);
            return Ok(category);
        }

        /// <summary>
        /// Updates a category if it belongs to the authenticated user.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] GoalCategoryUpdateDto dto)
        {
            var userId = GetCurrentUserId();
            await ApplicationService.GoalCategoryService.UpdateCategoryAsync(id, dto, userId);
            
            return NoContent();
        }

        /// <summary>
        /// Deletes a category if it belongs to the authenticated user.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = GetCurrentUserId();
            await ApplicationService.GoalCategoryService.DeleteCategoryAsync(id, userId);
            
            return NoContent();
        }

        /// <summary>
        /// Extracts the current user's ID from the JWT token claims.
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                throw new EntityNotAuthorizedException("User", "User ID claim not found in token.");
            }
            return int.Parse(userIdString);
        }
    }
}
