using GoalTrackerApp.Data;
using GoalTrackerApp.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly GoalTrackerAppDbContext _context;

    // Private repository fields
    // -----------------------------------------------------
    // These fields store a single instance of each repository.
    // They are kept private to prevent external modification
    // and to maintain full control inside the UnitOfWork.
    private UserRepository _userRepository;
    private GoalRepository _goalRepository;
    private GoalCategoryRepository _goalCategoryRepository;
    private PasswordResetTokenRepository _passwordResetTokenRepository;

    public UnitOfWork(GoalTrackerAppDbContext context)
    {
        _context = context;
    }

    // Repository properties (Lazy-loaded)
    // -----------------------------------------------------
    // Each property initializes its repository only once.
    // This ensures that the same repository instance is reused
    // throughout the lifetime of the UnitOfWork.
    // Without this, a new instance would be created on every call.
    public UserRepository UserRepository =>
        _userRepository ??= new UserRepository(_context);

    public GoalRepository GoalRepository =>
        _goalRepository ??= new GoalRepository(_context);

    public GoalCategoryRepository GoalCategoryRepository =>
        _goalCategoryRepository ??= new GoalCategoryRepository(_context);

    public PasswordResetTokenRepository PasswordResetTokenRepository =>
        _passwordResetTokenRepository ??= new PasswordResetTokenRepository(_context);

    // Saves all changes made through the repositories
    // -----------------------------------------------------
    // The UnitOfWork coordinates the commit for all repository actions.
    public async Task<bool> SaveAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}