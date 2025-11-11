namespace GoalTrackerApp.Dto;

/// <summary>
/// JWT authentication token data transfer object.
/// Contains the token and associated user information.
/// </summary>
public class JwtTokenDto
{
    /// <summary>
    /// The JWT token string
    /// </summary>
    public string? Token { get; set; } 
    
    /// <summary>
    /// The authenticated user's username
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// The user's role (e.g., "User", "Admin")
    /// </summary>
    public string Role { get; set; } = string.Empty;
    
    /// <summary>
    /// Token expiration date and time (UTC)
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}