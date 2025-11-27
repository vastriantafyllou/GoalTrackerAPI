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
    }
