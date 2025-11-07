using GoalTrackerApp.Data;

namespace GoalTrackerApp.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly GoalTrackerAppDbContext _context;
    
    public UnitOfWork(GoalTrackerAppDbContext context)
    {
        _context = context;
    }
    
    // implement repositories as properties
    public UserRepository UserRepository => new(_context);
    public GoalRepository GoalRepository => new(_context);
    public GoalCategoryRepository GoalCategoryRepository => new(_context);
    
    public async Task <bool> SaveAsync()
    {
        return await _context.SaveChangesAsync() > 0;  // commit & rollback if fails
    }
}