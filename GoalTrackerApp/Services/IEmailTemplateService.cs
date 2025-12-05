namespace GoalTrackerApp.Services;

public interface IEmailTemplateService
{
    Task<string> LoadTemplateAsync(string templateName, Dictionary<string, string> placeholders);
}
