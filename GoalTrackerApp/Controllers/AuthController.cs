using Microsoft.AspNetCore.Mvc;
using GoalTrackerApp.Dto;
using GoalTrackerApp.Exceptions;
using GoalTrackerApp.Services;

namespace GoalTrackerApp.Controllers;

    [ApiController]
    [Route("api/auth")]
    public class AuthController : BaseController
    {
        private readonly IConfiguration _configuration;

        public AuthController(IApplicationService applicationService, IConfiguration configuration) 
            : base(applicationService)
        {
            _configuration = configuration;
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
        public async Task<IActionResult> SendPasswordRecoveryEmail(string email)
        {
            await ApplicationService.UserService.SendPasswordRecoveryEmailAsync(email);
            return Ok(new { message = "Password recovery email sent successfully" });
        }

        /// <summary>
        /// Reset password
        /// </summary>
        [HttpPost("/api/reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetDto resetDto)
        {
            await ApplicationService.UserService.ResetPasswordAsync(resetDto);
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
