namespace GoalTrackerApp.Services;

public interface IEmailService
{
    Task SendPasswordRecoveryEmailAsync(string toEmail, string resetLink, string username);
    Task SendPasswordResetConfirmationAsync(string toEmail, string username);
}
