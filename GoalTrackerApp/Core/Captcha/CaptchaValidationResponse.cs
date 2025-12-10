using System.Text.Json.Serialization;

namespace GoalTrackerApp.Core.Captcha;

public class CaptchaValidationResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("challenge_ts")]
    public string? ChallengeTimestamp { get; set; }
    
    [JsonPropertyName("hostname")]
    public string? Hostname { get; set; }
    
    [JsonPropertyName("error-codes")]
    public List<string>? ErrorCodes { get; set; }
    
    [JsonPropertyName("score")]
    public double? Score { get; set; }
    
    [JsonPropertyName("action")]
    public string? Action { get; set; }
}
