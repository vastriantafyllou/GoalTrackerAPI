namespace GoalTrackerApp.Dto;

public class PasswordResetTokenValidationDto
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
}
