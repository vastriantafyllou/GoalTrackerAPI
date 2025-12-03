namespace GoalTrackerApp.Repositories;

public interface IUnitOfWork
{
    UserRepository UserRepository { get; }
    GoalRepository GoalRepository { get; }
    GoalCategoryRepository GoalCategoryRepository { get; }
    PasswordResetTokenRepository PasswordResetTokenRepository { get; }
    Task<bool> SaveAsync();
}