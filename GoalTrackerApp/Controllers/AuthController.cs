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
        /// Registers a new user in the system
        /// </summary>
        /// <param name="userSignupDto">The user registration details</param>
        /// <returns>The newly created user's information</returns>
        /// <response code="201">User successfully registered</response>
        /// <response code="400">Invalid input data</response>
        /// <exception cref="InvalidRegistrationException">Thrown when registration data is invalid</exception>
        [HttpPost("register")] 
        [ProducesResponseType(typeof(UserReadOnlyDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserReadOnlyDto>> RegisterUserAsync([FromBody] UserSignupDto userSignupDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(e => e.Value?.Errors.Any() == true)
                    .Select(e => new { 
                        Field = e.Key, 
                        Errors = e.Value!.Errors.Select(er => er.ErrorMessage).ToArray()
                    })
                    .ToList();

                var errorMessage = string.Join("; ", errors.SelectMany(e => e.Errors));
                throw new InvalidArgumentException("User", $"User registration failed: {errorMessage}");
            }

            var returnedUserDto = await ApplicationService.UserService.SignUpUserAsync(userSignupDto);
            return CreatedAtAction(nameof(RegisterUserAsync), new { id = returnedUserDto.Id }, returnedUserDto);
        }


        /// <summary>
        /// Handles user authentication by validating credentials and returning a JWT token
        /// </summary>
        /// <param name="credentials">The user's login credentials (username/password)</param>
        /// <returns>A JWT token for authenticated requests</returns>
        /// <response code="200">Returns the JWT token upon successful authentication</response>
        /// <response code="401">Invalid username or password</response>
        /// <exception cref="EntityNotAuthorizedException">Thrown when authentication fails</exception>
        [HttpPost("login")]
        [ProducesResponseType(typeof(JwtTokenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<JwtTokenDto>> LoginUserAsync([FromBody] UserLoginDto credentials)
        {
            var user = await ApplicationService.UserService.VerifyAndGetUserAsync(credentials) 
                ?? throw new EntityNotAuthorizedException("User", "Invalid username or password.");

            // Determine token expiration based on KeepLoggedIn flag
            var expirationHours = credentials.KeepLoggedIn ? 168 : 8; // 7 days or 8 hours
            
            var jwtSettings = _configuration.GetSection("Authentication");
            var token = ApplicationService.UserService.CreateUserToken(
                user.Id, 
                user.Username, 
                user.Email, 
                user.UserRole,
                expirationHours,
                jwtSettings["SecretKey"]!,
                jwtSettings["Issuer"]!,
                jwtSettings["Audience"]!);
            
            var userToken = new JwtTokenDto 
            { 
                Token = token,
                Username = user.Username,
                Role = user.UserRole.ToString(),
                ExpiresAt = DateTime.UtcNow.AddHours(expirationHours) 
            };
            
            return Ok(userToken);
        }
    }
