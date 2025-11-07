using AutoMapper;
using GoalTrackerApp.Core.Enums;
using GoalTrackerApp.Data;
using GoalTrackerApp.Dto;
using GoalTrackerApp.Exceptions;
using GoalTrackerApp.Repositories;
using Serilog;

namespace GoalTrackerApp.Services
{
    public class GoalService : IGoalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GoalService> _logger = new LoggerFactory().AddSerilog().CreateLogger<GoalService>();

        public GoalService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // --- 1. CREATE ---
        public async Task<GoalReadOnlyDto> CreateGoalAsync(GoalCreateDto dto, int userId)
        {
            var goal = _mapper.Map<Goal>(dto);
            
            goal.UserId = userId;
            goal.GoalStatus = GoalStatus.InProgress;

            // Validate category if provided
            if (dto.GoalCategoryId.HasValue)
            {
                var category = await _unitOfWork.GoalCategoryRepository.GetAsync(dto.GoalCategoryId.Value);
                if (category == null || category.UserId != userId)
                {
                    throw new EntityNotFoundException("GoalCategory", 
                        $"Category with ID: {dto.GoalCategoryId} not found or does not belong to user.");
                }
            }

            try
            {
                await _unitOfWork.GoalRepository.AddAsync(goal);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("New Goal created with ID: {GoalId} for User: {UserId}", goal.Id, userId);
                
                return _mapper.Map<GoalReadOnlyDto>(goal);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating goal for user {UserId}", userId);
                throw new ServerException("Server", "Could not create goal.");
            }
        }

        // --- 2. READ ALL (for a user) ---
        public async Task<IEnumerable<GoalReadOnlyDto>> GetGoalsForUserAsync(int userId)
        {
            var goals = await _unitOfWork.GoalRepository.GetGoalsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<GoalReadOnlyDto>>(goals);
        }

        // --- 3. READ ONE ---
        public async Task<GoalReadOnlyDto> GetGoalByIdAsync(int goalId, int userId)
        {
            var goal = await _unitOfWork.GoalRepository.GetAsync(goalId);

            if (goal == null)
            {
                throw new EntityNotFoundException("Goal", $"Goal with ID: {goalId} not found.");
            }

            // Security check: ensure the goal belongs to the requesting user
            if (goal.UserId != userId)
            {
                _logger.LogWarning("User {UserId} tried to access unauthorized goal {GoalId}", userId, goalId);
                throw new EntityNotFoundException("Goal", $"Goal with ID: {goalId} not found.");
            }

            return _mapper.Map<GoalReadOnlyDto>(goal);
        }

        // --- 4. UPDATE ---
        public async Task UpdateGoalAsync(int goalId, GoalUpdateDto dto, int userId)
        {
            var goal = await _unitOfWork.GoalRepository.GetAsync(goalId);

            if (goal == null)
            {
                throw new EntityNotFoundException("Goal", $"Goal with ID: {goalId} not found.");
            }

            // Security check: ensure the goal belongs to the requesting user
            if (goal.UserId != userId)
            {
                _logger.LogWarning("User {UserId} tried to update unauthorized goal {GoalId}", userId, goalId);
                throw new EntityNotFoundException("Goal", $"Goal with ID: {goalId} not found.");
            }

            // Validate category if provided
            if (dto.GoalCategoryId.HasValue)
            {
                var category = await _unitOfWork.GoalCategoryRepository.GetAsync(dto.GoalCategoryId.Value);
                if (category == null || category.UserId != userId)
                {
                    throw new EntityNotFoundException("GoalCategory", 
                        $"Category with ID: {dto.GoalCategoryId} not found or does not belong to user.");
                }
            }

            _mapper.Map(dto, goal);
            
            await _unitOfWork.GoalRepository.UpdateAsync(goal);
            await _unitOfWork.SaveAsync();
            _logger.LogInformation("Goal {GoalId} updated by user {UserId}", goalId, userId);
        }

        // --- 5. DELETE ---
        public async Task DeleteGoalAsync(int goalId, int userId)
        {
            var goal = await _unitOfWork.GoalRepository.GetAsync(goalId);

            if (goal == null)
            {
                throw new EntityNotFoundException("Goal", $"Goal with ID: {goalId} not found.");
            }

            // Security check: ensure the goal belongs to the requesting user
            if (goal.UserId != userId)
            {
                _logger.LogWarning("User {UserId} tried to delete unauthorized goal {GoalId}", userId, goalId);
                throw new EntityNotFoundException("Goal", $"Goal with ID: {goalId} not found.");
            }

            await _unitOfWork.GoalRepository.DeleteAsync(goalId);
            await _unitOfWork.SaveAsync();
            _logger.LogInformation("Goal {GoalId} deleted by user {UserId}", goalId, userId);
        }
    }
}