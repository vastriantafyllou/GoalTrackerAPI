using System.ComponentModel.DataAnnotations;

namespace GoalTrackerApp.Dto;

public class PasswordResetDto
{
    [Required(ErrorMessage = "Token is required.")]
    public string? Token { get; set; }
    
    [Required(ErrorMessage = "New password is required.")]
    [MinLength(12, ErrorMessage = "Password must be at least 12 characters long.")]
    [RegularExpression(@"(?=.*?[A-Z])(?=.*?[a-z])(?=.*?\d)(?=.*?\W)^.{12,}$",
        ErrorMessage = "Password must be at least 12 characters and contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
    public string? NewPassword { get; set; }
}