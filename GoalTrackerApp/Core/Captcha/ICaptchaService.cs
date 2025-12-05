namespace GoalTrackerApp.Core.Captcha;

public interface ICaptchaService
{
    Task<bool> ValidateCaptchaAsync(string captchaToken);
}
