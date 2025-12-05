using Microsoft.AspNetCore.Mvc;
using GoalTrackerApp.Dto;
using GoalTrackerApp.Exceptions;
using GoalTrackerApp.Services;
using GoalTrackerApp.Core.RateLimiting;
using GoalTrackerApp.Core.Captcha;

namespace GoalTrackerApp.Controllers;

    [ApiController]
    [Route("api/auth")]
    public class AuthController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly PasswordRecoveryRateLimiter _rateLimiter;
        private readonly ICaptchaService _captchaService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IApplicationService applicationService, 
            IConfiguration configuration,
            PasswordRecoveryRateLimiter rateLimiter,
            ICaptchaService captchaService,
            IEmailService emailService,
            ILogger<AuthController> logger) 
            : base(applicationService)
        {
            _configuration = configuration;
            _rateLimiter = rateLimiter;
            _captchaService = captchaService;
            _emailService = emailService;
            _logger = logger;
        }
        
        /// <summary>
        /// Authenticate user and get token
        /// </summary>
        [HttpPost ("login/access-token")]
        public async Task<ActionResult<JwtTokenDto>> LoginUserAsync([FromBody] UserLoginDto credentials)
        {
            var user = await ApplicationService.UserService.VerifyAndGetUserAsync(credentials) 
                ?? throw new EntityNotAuthorizedException("User", "Bad Credentials");
            
            if (user.IsDeleted)
            {
                throw new UnauthorizedAccessException("This account has been deactivated.");
            }
            
            var token = ApplicationService.UserService.CreateUserToken(
                user.Id, 
                user.Username, 
                user.Email, 
                user.UserRole, 
                _configuration["Authentication:SecretKey"]!);
            
            JwtTokenDto userToken = new JwtTokenDto 
            { 
                Token = token,
                Username = user.Username,
                Role = user.UserRole.ToString(),
                ExpiresAt = DateTime.UtcNow.AddHours(3) 
            };
            return Ok(userToken);
        }

        /// <summary>
        /// Send recovery email
        /// </summary>
        [HttpPost("/api/password-recovery/{email}")]
        public async Task<IActionResult> SendPasswordRecoveryEmail(string email, [FromQuery] string? captchaToken = null)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            if (!_rateLimiter.IsAllowed(email))
            {
                var retryAfter = _rateLimiter.GetRetryAfter(email);
                _logger.LogWarning("Rate limit exceeded for password recovery: {Email} from {IP}", email, ipAddress);
                return StatusCode(429, new 
                { 
                    message = "Too many password recovery requests. Please try again later.",
                    retryAfter = retryAfter?.TotalMinutes
                });
            }

            if (!string.IsNullOrEmpty(captchaToken))
            {
                var captchaValid = await _captchaService.ValidateCaptchaAsync(captchaToken);
                if (!captchaValid)
                {
                    _logger.LogWarning("Invalid CAPTCHA for password recovery: {Email}", email);
                    return BadRequest(new { message = "Invalid CAPTCHA token" });
                }
            }

            _logger.LogInformation("Password recovery requested for {Email} from {IP}", email, ipAddress);
            
            await ApplicationService.UserService.SendPasswordRecoveryEmailAsync(email);
            
            var frontendUrl = _configuration["PasswordReset:FrontendResetUrl"] ?? "http://localhost:5173/reset-password";
            var user = await ApplicationService.UserService.GetUserByEmailAsync(email);
            
            if (user != null)
            {
                var tokens = await ApplicationService.UserService.GetActivePasswordResetTokensAsync(user.Id);
                var latestToken = tokens.FirstOrDefault();
                
                if (latestToken != null)
                {
                    var resetLink = $"{frontendUrl}?token={latestToken.Token}";
                    await _emailService.SendPasswordRecoveryEmailAsync(email, resetLink, user.Username);
                }
            }
            
            return Ok(new { message = "If this email exists, a password recovery link has been sent" });
        }

        /// <summary>
        /// Reset password
        /// </summary>
        [HttpPost("/api/reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetDto resetDto)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            _logger.LogInformation("Password reset attempt from {IP} with token", ipAddress);
            
            var tokenEntity = await ApplicationService.UserService.GetPasswordResetTokenByValueAsync(resetDto.Token!);
            if (tokenEntity != null)
            {
                var user = await ApplicationService.UserService.GetUserByIdAsync(tokenEntity.UserId);
                if (user != null)
                {
                    await ApplicationService.UserService.ResetPasswordAsync(resetDto);
                    
                    await _emailService.SendPasswordResetConfirmationAsync(user.Email, user.Username);
                }
            }
            
            _logger.LogInformation("Password reset successful from {IP}", ipAddress);
            return Ok(new { message = "Password reset successfully" });
        }

        /// <summary>
        /// Validate reset token
        /// </summary>
        [HttpGet("/api/reset-password/{token}")]
        public async Task<ActionResult<PasswordResetTokenValidationDto>> ValidateResetToken(string token)
        {
            var result = await ApplicationService.UserService.ValidateResetTokenAsync(token);
            return Ok(result);
        }
    }
