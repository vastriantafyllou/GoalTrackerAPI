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
        private readonly ILogger<GoalService> _logger;

        public GoalService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GoalService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<GoalReadOnlyDto> CreateGoalAsync(GoalCreateDto dto, int userId)
        {
            var goal = _mapper.Map<Goal>(dto);
            
            // Set ownership and default status
            goal.UserId = userId;
            goal.GoalStatus = GoalStatus.InProgress;

            try
            {
                await _unitOfWork.GoalRepository.AddAsync(goal);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Goal created: ID={GoalId}, User={UserId}", goal.Id, userId);
                
                return _mapper.Map<GoalReadOnlyDto>(goal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating goal for user {UserId}", userId);
                throw new ServerException("Server", "Could not create goal.");
            }
        }

        public async Task<IEnumerable<GoalReadOnlyDto>> GetGoalsForUserAsync(int userId)
        {
            var goals = await _unitOfWork.GoalRepository.GetGoalsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<GoalReadOnlyDto>>(goals);
        }

        public async Task<GoalReadOnlyDto> GetGoalByIdAsync(int goalId, int userId)
        {
            var goal = await _unitOfWork.GoalRepository.GetAsync(goalId);

            if (goal == null)
            {
                throw new EntityNotFoundException("Goal", $"Goal with ID {goalId} not found.");
            }

            // Authorization check: ensure user owns this goal
            if (goal.UserId != userId)
            {
                _logger.LogWarning("Unauthorized access attempt: User {UserId} tried to access Goal {GoalId}", 
                    userId, goalId);
                // Return same error as not found to avoid information disclosure
                throw new EntityNotFoundException("Goal", $"Goal with ID {goalId} not found.");
            }

            return _mapper.Map<GoalReadOnlyDto>(goal);
        }

        public async Task UpdateGoalAsync(int goalId, GoalUpdateDto dto, int userId)
        {
            var goal = await _unitOfWork.GoalRepository.GetAsync(goalId);

            if (goal == null)
            {
                throw new EntityNotFoundException("Goal", $"Goal with ID {goalId} not found.");
            }

            // Authorization check: ensure user owns this goal
            if (goal.UserId != userId)
            {
                _logger.LogWarning("Unauthorized update attempt: User {UserId} tried to update Goal {GoalId}", 
                    userId, goalId);
                throw new EntityNotFoundException("Goal", $"Goal with ID {goalId} not found.");
            }

            // Map updated values to existing entity
            _mapper.Map(dto, goal);
            
            await _unitOfWork.GoalRepository.UpdateAsync(goal);
            await _unitOfWork.SaveAsync();
            
            _logger.LogInformation("Goal updated: ID={GoalId}, User={UserId}", goalId, userId);
        }

        public async Task DeleteGoalAsync(int goalId, int userId)
        {
            var goal = await _unitOfWork.GoalRepository.GetAsync(goalId);

            if (goal == null)
            {
                throw new EntityNotFoundException("Goal", $"Goal with ID {goalId} not found.");
            }

            // Authorization check: ensure user owns this goal
            if (goal.UserId != userId)
            {
                _logger.LogWarning("Unauthorized delete attempt: User {UserId} tried to delete Goal {GoalId}", 
                    userId, goalId);
                throw new EntityNotFoundException("Goal", $"Goal with ID {goalId} not found.");
            }

            await _unitOfWork.GoalRepository.DeleteAsync(goalId);
            await _unitOfWork.SaveAsync();
            
            _logger.LogInformation("Goal deleted: ID={GoalId}, User={UserId}", goalId, userId);
        }
    }
}