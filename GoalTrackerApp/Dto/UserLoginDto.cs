using System.ComponentModel.DataAnnotations;

namespace GoalTrackerApp.Dto;

/// <summary>
/// Data transfer object for user login credentials.
/// </summary>
public record UserLoginDto
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Username must be between 2 and 50 characters.")]
    public string? Username { get; set; }
    
    [Required(ErrorMessage = "Password is required.")]
    [RegularExpression(@"(?=.*?[A-Z])(?=.*?[a-z])(?=.*?\d)(?=.*?\W)^.{8,}$",
        ErrorMessage = "Password must contain at least one uppercase, one lowercase, one digit, and one special character.")]
    public string? Password { get; set; }
    
    /// <summary>
    /// Indicates whether the user wants to remain logged in for an extended period.
    /// This can be used to adjust token expiration time.
    /// </summary>
    public bool KeepLoggedIn { get; set; }
}
