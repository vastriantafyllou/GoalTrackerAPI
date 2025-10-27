using GoalTrackerApp.Data;
using Microsoft.EntityFrameworkCore;

namespace GoalTrackerApp.Repositories;

// The generic T must be a class that inherits from our BaseEntity
public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
{
    protected readonly GoalTrackerAppDbContext context;
    protected readonly DbSet<T> dbSet;
    
    public BaseRepository(GoalTrackerAppDbContext context)
    {
        this.context = context;
        dbSet = this.context.Set<T>();
    }
    
    public virtual async Task AddAsync(T entity) => await dbSet.AddAsync(entity);
    
    public virtual async Task AddRangeAsync(IEnumerable<T> entities) => await dbSet.AddRangeAsync(entities);
    

    public virtual Task UpdateAsync(T entity)
    {
        dbSet.Attach(entity);
        context.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        T? existingEntity = await GetAsync(id);
        if (existingEntity == null) return false;
        existingEntity.IsDeleted = true;
        existingEntity.DeletedAt = DateTime.UtcNow; // Soft delete
        existingEntity.ModifiedAt = DateTime.UtcNow;
        return true;
    }

    public virtual async Task<T?> GetAsync(int id) => await dbSet.FindAsync(id);
    
    public virtual async Task<IEnumerable<T>> GetAllAsync() => await dbSet.ToListAsync();
    
    public virtual async Task<int> GetCountAsync() => await dbSet.CountAsync();
    
}