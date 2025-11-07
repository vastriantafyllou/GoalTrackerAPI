using AutoMapper;
using GoalTrackerApp.Data;
using GoalTrackerApp.Dto;
using GoalTrackerApp.Exceptions;
using GoalTrackerApp.Repositories;
using Serilog;

namespace GoalTrackerApp.Services
{
    public class GoalCategoryService : IGoalCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GoalCategoryService> _logger = new LoggerFactory().AddSerilog().CreateLogger<GoalCategoryService>();

        public GoalCategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // --- 1. CREATE ---
        public async Task<GoalCategoryReadOnlyDto> CreateCategoryAsync(GoalCategoryCreateDto dto, int userId)
        {
            var category = _mapper.Map<GoalCategory>(dto);
            category.UserId = userId;

            try
            {
                // Check if a category with the same name already exists for this user
                var existingCategories = await _unitOfWork.GoalCategoryRepository.GetCategoriesByUserIdAsync(userId);
                if (existingCategories.Any(c => c.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new EntityAlreadyExistsException("GoalCategory", 
                        $"Category with name '{dto.Name}' already exists for this user.");
                }

                await _unitOfWork.GoalCategoryRepository.AddAsync(category);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("New GoalCategory created with ID: {CategoryId} for User: {UserId}", 
                    category.Id, userId);

                return _mapper.Map<GoalCategoryReadOnlyDto>(category);
            }
            catch (EntityAlreadyExistsException)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating category for user {UserId}", userId);
                throw new ServerException("Server", "Could not create category.");
            }
        }

        // --- 2. READ ALL (for a user) ---
        public async Task<IEnumerable<GoalCategoryReadOnlyDto>> GetCategoriesForUserAsync(int userId)
        {
            var categories = await _unitOfWork.GoalCategoryRepository.GetCategoriesByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<GoalCategoryReadOnlyDto>>(categories);
        }

        // --- 3. READ ONE ---
        public async Task<GoalCategoryReadOnlyDto> GetCategoryByIdAsync(int categoryId, int userId)
        {
            var category = await _unitOfWork.GoalCategoryRepository.GetAsync(categoryId);

            if (category == null)
            {
                throw new EntityNotFoundException("GoalCategory", $"Category with ID: {categoryId} not found.");
            }

            // Security check: ensure the category belongs to the requesting user
            if (category.UserId != userId)
            {
                _logger.LogWarning("User {UserId} tried to access unauthorized category {CategoryId}", 
                    userId, categoryId);
                throw new EntityNotFoundException("GoalCategory", $"Category with ID: {categoryId} not found.");
            }

            return _mapper.Map<GoalCategoryReadOnlyDto>(category);
        }

        // --- 4. UPDATE ---
        public async Task UpdateCategoryAsync(int categoryId, GoalCategoryUpdateDto dto, int userId)
        {
            var category = await _unitOfWork.GoalCategoryRepository.GetAsync(categoryId);

            if (category == null)
            {
                throw new EntityNotFoundException("GoalCategory", $"Category with ID: {categoryId} not found.");
            }

            // Security check: ensure the category belongs to the requesting user
            if (category.UserId != userId)
            {
                _logger.LogWarning("User {UserId} tried to update unauthorized category {CategoryId}", 
                    userId, categoryId);
                throw new EntityNotFoundException("GoalCategory", $"Category with ID: {categoryId} not found.");
            }

            // Check if the new name already exists
            var existingCategories = await _unitOfWork.GoalCategoryRepository.GetCategoriesByUserIdAsync(userId);
            if (existingCategories.Any(c => c.Id != categoryId && 
                                           c.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new EntityAlreadyExistsException("GoalCategory", 
                    $"Category with name '{dto.Name}' already exists for this user.");
            }

            _mapper.Map(dto, category);

            await _unitOfWork.GoalCategoryRepository.UpdateAsync(category);
            await _unitOfWork.SaveAsync();
            
            _logger.LogInformation("Category {CategoryId} updated by user {UserId}", categoryId, userId);
        }

        // --- 5. DELETE ---
        public async Task DeleteCategoryAsync(int categoryId, int userId)
        {
            var category = await _unitOfWork.GoalCategoryRepository.GetAsync(categoryId);

            if (category == null)
            {
                throw new EntityNotFoundException("GoalCategory", $"Category with ID: {categoryId} not found.");
            }

            // Security check: ensure the category belongs to the requesting user
            if (category.UserId != userId)
            {
                _logger.LogWarning("User {UserId} tried to delete unauthorized category {CategoryId}", 
                    userId, categoryId);
                throw new EntityNotFoundException("GoalCategory", $"Category with ID: {categoryId} not found.");
            }

            await _unitOfWork.GoalCategoryRepository.DeleteAsync(categoryId);
            await _unitOfWork.SaveAsync();
            
            _logger.LogInformation("Category {CategoryId} deleted by user {UserId}", categoryId, userId);
        }
    }
}
