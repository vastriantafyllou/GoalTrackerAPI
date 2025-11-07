namespace GoalTrackerApp.Data;

public class GoalCategory : BaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!; 
    
    public virtual ICollection<Goal> Goals { get; set; } = new HashSet<Goal>();
}