using GoalTrackerApp.Data;

namespace GoalTrackerApp.Repositories;

public interface IPasswordResetTokenRepository : IBaseRepository<PasswordResetToken>
{
    Task<PasswordResetToken?> GetByTokenAsync(string token);
    Task<List<PasswordResetToken>> GetActiveTokensByUserIdAsync(int userId);
}
