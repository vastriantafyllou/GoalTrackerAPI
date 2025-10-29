using GoalTrackerApp.Data;

namespace GoalTrackerApp.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly GoalTrackerAppDbContext context;
    
    public UnitOfWork(GoalTrackerAppDbContext context)
    {
        this.context = context;
    }
    
    // implement repositories as properties
    public UserRepository UserRepository => new(context);
    public GoalRepository GoalRepository => new(context);
    public GoalCategoryRepository GoalCategoryRepository => new(context);
    
    public async Task <bool> SaveAsync()
    {
        return await context.SaveChangesAsync() > 0;  // commit & rollback if fails
    }
}