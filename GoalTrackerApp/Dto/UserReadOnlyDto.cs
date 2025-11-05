namespace GoalTrackerApp.Dto;

/// <summary>
/// Read-only user data transfer object.
/// Used for returning user information without sensitive data.
/// </summary>
public record UserReadOnlyDto
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Firstname { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public string UserRole { get; set; } = null!;
}
