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
        /// Handles user authentication by validating credentials and returning a JWT token
        /// </summary>
        /// <param name="credentials">The user's login credentials (username/password)</param>
        /// <returns>A JWT token for authenticated requests</returns>
        /// <response code="200">Returns the JWT token upon successful authentication</response>
        /// <response code="401">Invalid username or password</response>
        /// <exception cref="EntityNotAuthorizedException">Thrown when authentication fails</exception>
        [HttpPost("login/access-token")]
        public async Task<ActionResult<JwtTokenDto>> LoginUserAsync([FromBody] UserLoginDto credentials)
        {
            var user = await ApplicationService.UserService.VerifyAndGetUserAsync(credentials) 
                ?? throw new EntityNotAuthorizedException("User", "Bad Credentials");

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
