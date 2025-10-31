namespace GoalTrackerApp.Dto;

public class JwtTokenDto
{
    public string? Token { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}