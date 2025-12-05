using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace GoalTrackerApp.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly IEmailTemplateService _templateService;

    public EmailService(
        IConfiguration configuration, 
        ILogger<EmailService> logger,
        IEmailTemplateService templateService)
    {
        _configuration = configuration;
        _logger = logger;
        _templateService = templateService;
    }

    public async Task SendPasswordRecoveryEmailAsync(string toEmail, string resetLink, string username)
    {
        var subject = "Password Recovery Instructions - Goal Tracker";
        
        var placeholders = new Dictionary<string, string>
        {
            { "Username", username },
            { "ResetLink", resetLink }
        };
        
        var body = await _templateService.LoadTemplateAsync("PasswordRecovery", placeholders);
        await SendEmailAsync(toEmail, subject, body);
        _logger.LogInformation("Password recovery email sent to {Email}", toEmail);
    }

    public async Task SendPasswordResetConfirmationAsync(string toEmail, string username)
    {
        var subject = "Password Successfully Reset - Goal Tracker";
        
        var placeholders = new Dictionary<string, string>
        {
            { "Username", username }
        };
        
        var body = await _templateService.LoadTemplateAsync("PasswordResetConfirmation", placeholders);
        await SendEmailAsync(toEmail, subject, body);
        _logger.LogInformation("Password reset confirmation email sent to {Email}", toEmail);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var smtpServer = emailSettings["SmtpServer"];
            var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
            var senderEmail = emailSettings["SenderEmail"];
            var senderName = emailSettings["SenderName"];
            var username = emailSettings["Username"];
            var password = emailSettings["Password"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            
            _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
            throw;
        }
    }
}
