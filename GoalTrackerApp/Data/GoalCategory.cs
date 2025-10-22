namespace GoalTrackerApp.Data;

public class GoalCategory : BaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    
    // A User has personal GoalCategories (One-to-Many)
    public int UserId { get; set; }  // foreign key
    public virtual User User { get; set; } = null!; 
    
    // GoalCategories - Goals (One-to-Many)
    public virtual ICollection<Goal> Goals { get; set; } = new HashSet<Goal>();
}