using System.ComponentModel.DataAnnotations;

namespace GoalTrackerApp.Dto;

public class PasswordResetDto
{
    [Required]
    public string? Token { get; set; }
    
    [Required]
    [MinLength(6)]
    public string? NewPassword { get; set; }
}
