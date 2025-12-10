using System.Text.Json;

namespace GoalTrackerApp.Core.Captcha;

public class CaptchaService : ICaptchaService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CaptchaService> _logger;
    private readonly HttpClient _httpClient;
    private const string RecaptchaVerifyUrl = "https://www.google.com/recaptcha/api/siteverify";
    private const double MinAcceptableScore = 0.5;

    public CaptchaService(IConfiguration configuration, ILogger<CaptchaService> logger, HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<bool> ValidateCaptchaAsync(string captchaToken)
    {
        if (string.IsNullOrWhiteSpace(captchaToken))
        {
            _logger.LogWarning("CAPTCHA validation failed: token is null or empty");
            return false;
        }

        var captchaEnabled = _configuration.GetValue<bool>("CaptchaSettings:Enabled");
        if (!captchaEnabled)
        {
            _logger.LogInformation("CAPTCHA validation skipped (disabled in configuration)");
            return true;
        }

        var secretKey = _configuration["CaptchaSettings:SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey) || secretKey.StartsWith("{"))
        {
            _logger.LogWarning("CAPTCHA validation failed: SecretKey not configured properly. Using fallback mode.");
            return true;
        }

        try
        {
            _logger.LogInformation("Validating CAPTCHA token with Google reCAPTCHA API");
            
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", secretKey),
                new KeyValuePair<string, string>("response", captchaToken)
            });

            var response = await _httpClient.PostAsync(RecaptchaVerifyUrl, requestContent);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("CAPTCHA validation request failed with status code: {StatusCode}", response.StatusCode);
                return false;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("reCAPTCHA API response: {Response}", jsonResponse);

            var validationResponse = JsonSerializer.Deserialize<CaptchaValidationResponse>(jsonResponse, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (validationResponse == null)
            {
                _logger.LogError("Failed to deserialize CAPTCHA validation response");
                return false;
            }

            if (!validationResponse.Success)
            {
                var errorCodes = validationResponse.ErrorCodes != null && validationResponse.ErrorCodes.Any()
                    ? string.Join(", ", validationResponse.ErrorCodes)
                    : "No error codes provided";
                
                _logger.LogWarning("CAPTCHA validation failed. Error codes: {ErrorCodes}", errorCodes);
                return false;
            }

            if (validationResponse.Score.HasValue)
            {
                _logger.LogInformation("reCAPTCHA v3 score: {Score}", validationResponse.Score.Value);
                
                if (validationResponse.Score.Value < MinAcceptableScore)
                {
                    _logger.LogWarning("CAPTCHA score too low: {Score} (minimum: {MinScore})", 
                        validationResponse.Score.Value, MinAcceptableScore);
                    return false;
                }
            }

            _logger.LogInformation("CAPTCHA validation successful. Hostname: {Hostname}, Challenge timestamp: {Timestamp}", 
                validationResponse.Hostname, validationResponse.ChallengeTimestamp);
            
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error occurred while validating CAPTCHA");
            return false;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse CAPTCHA validation response");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during CAPTCHA validation");
            return false;
        }
    }
}
