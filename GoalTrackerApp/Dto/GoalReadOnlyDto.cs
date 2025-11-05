namespace GoalTrackerApp.Dto;

/// <summary>
/// Read-only goal data transfer object.
/// Used for returning goal information to clients.
/// </summary>
public class GoalReadOnlyDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    /// <summary>
    /// Goal status as string (e.g., "InProgress", "Completed", "Cancelled")
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    public DateTime? DueDate { get; set; }
    public DateTime CreatedDate { get; set; }
}
