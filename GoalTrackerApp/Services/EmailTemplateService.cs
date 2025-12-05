using System.Text.RegularExpressions;

namespace GoalTrackerApp.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailTemplateService> _logger;

    public EmailTemplateService(
        IWebHostEnvironment environment, 
        IConfiguration configuration,
        ILogger<EmailTemplateService> logger)
    {
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> LoadTemplateAsync(string templateName, Dictionary<string, string> placeholders)
    {
        try
        {
            var templatePath = Path.Combine(_environment.ContentRootPath, "EmailTemplates", $"{templateName}.html");
            
            if (!File.Exists(templatePath))
            {
                _logger.LogError("Email template not found: {TemplatePath}", templatePath);
                throw new FileNotFoundException($"Email template not found: {templateName}.html");
            }

            var template = await File.ReadAllTextAsync(templatePath);
            
            var defaultPlaceholders = GetDefaultPlaceholders();
            foreach (var (key, value) in defaultPlaceholders)
            {
                if (!placeholders.ContainsKey(key))
                {
                    placeholders[key] = value;
                }
            }

            foreach (var (key, value) in placeholders)
            {
                template = template.Replace($"{{{{{key}}}}}", value);
            }

            var unReplacedPlaceholders = Regex.Matches(template, @"\{\{(\w+)\}\}");
            if (unReplacedPlaceholders.Count > 0)
            {
                _logger.LogWarning("Unreplaced placeholders found in template {TemplateName}: {Placeholders}", 
                    templateName, 
                    string.Join(", ", unReplacedPlaceholders.Select(m => m.Value)));
            }

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading email template: {TemplateName}", templateName);
            throw;
        }
    }

    private Dictionary<string, string> GetDefaultPlaceholders()
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        var appSettings = _configuration.GetSection("AppSettings");

        return new Dictionary<string, string>
        {
            { "Year", DateTime.UtcNow.Year.ToString() },
            { "LogoUrl", appSettings["LogoUrl"] ?? "https://via.placeholder.com/120x40?text=Goal+Tracker" },
            { "AppUrl", appSettings["AppUrl"] ?? "https://goaltracker.com" },
            // { "SupportUrl", appSettings["SupportUrl"] ?? "https://goaltracker.com/support" },
            { "LoginUrl", appSettings["LoginUrl"] ?? "https://goaltracker.com/login" }
        };
    }
}
