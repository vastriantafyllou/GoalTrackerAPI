using GoalTrackerApp.Data;
using Microsoft.EntityFrameworkCore;

namespace GoalTrackerApp.Repositories;

public class PasswordResetTokenRepository : BaseRepository<PasswordResetToken>, IPasswordResetTokenRepository
{
    public PasswordResetTokenRepository(GoalTrackerAppDbContext context) : base(context)
    {
    }

    public async Task<PasswordResetToken?> GetByTokenAsync(string token)
    {
        return await context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task<List<PasswordResetToken>> GetActiveTokensByUserIdAsync(int userId)
    {
        return await context.PasswordResetTokens
            .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
}
