using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using GoalTrackerApp.Core.Filters;
using GoalTrackerApp.Data;
using GoalTrackerApp.Dto;
using GoalTrackerApp.Models;
using GoalTrackerApp.Repositories;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using GoalTrackerApp.Core.Enums;
using GoalTrackerApp.Exceptions;
using GoalTrackerApp.Security;
using Microsoft.IdentityModel.Tokens;

namespace GoalTrackerApp.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PaginatedResult<UserReadOnlyDto>> GetPaginatedUsersFilteredAsync(int pageNumber, 
            int pageSize, UserFiltersDto userFiltersDto)
        {
            List<User> users = [];
            List<Expression<Func<User, bool>>> predicates = [];

            if (!string.IsNullOrEmpty(userFiltersDto.Username))
            {
                predicates.Add(u => u.Username == userFiltersDto.Username);
            }
            if (!string.IsNullOrEmpty(userFiltersDto.Email))
            {
                predicates.Add(u => u.Email == userFiltersDto.Email);
            }
            if (!string.IsNullOrEmpty(userFiltersDto.UserRole))
            {
                predicates.Add(u => u.UserRole.ToString() == userFiltersDto.UserRole);
            }

            var result = await _unitOfWork.UserRepository.GetUsersAsync(pageNumber, pageSize, predicates);
            
            var dtoResult = new PaginatedResult<UserReadOnlyDto>()
            {
                Data = result.Data.Select(u => new UserReadOnlyDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Firstname = u.Firstname,
                    Lastname = u.Lastname,
                    UserRole = u.UserRole.ToString()
                }).ToList(),
                TotalRecords = result.TotalRecords,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
            _logger.LogInformation("Retrieved {Count} users", dtoResult.Data.Count);
            return dtoResult;
        }

        public async Task<UserReadOnlyDto?> GetUserByUsernameAsync(string username)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    throw new EntityNotFoundException("User", $"User with username '{username}' not found.");
                }
                
                _logger.LogInformation("User found: {Username}", username);
                return _mapper.Map<UserReadOnlyDto>(user);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogError("Error retrieving user by username: {Username}. {Message}", username, ex.Message);
                throw;
            }
        }

        public async Task<User?> VerifyAndGetUserAsync(UserLoginDto credentials)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetUserAsync(credentials.Username, credentials.Password);

                if (user == null)
                {
                    _logger.LogWarning("Authentication failed for username: {Username}", credentials.Username);
                    return null;
                }

                _logger.LogInformation("User authenticated successfully: {Username}", credentials.Username);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for username: {Username}", credentials.Username);
                throw new ServerException("Server", "An error occurred during authentication.");
            }
        }
        
        public async Task<UserReadOnlyDto> GetUserByIdAsync(int id)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetAsync(id);
                if (user == null)
                {
                    throw new EntityNotFoundException("User", $"User with ID {id} not found.");
                }
                
                _logger.LogInformation("User found with ID: {Id}", id);
                return _mapper.Map<UserReadOnlyDto>(user);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogError("Error retrieving user by ID: {Id}. {Message}", id, ex.Message);
                throw;
            }
        }
        
        public string CreateUserToken(int userId, string username, string email, UserRole userRole, 
            int expirationHours, string appSecurityKey, string issuer, string audience)
        {
            if (string.IsNullOrWhiteSpace(appSecurityKey))
                throw new ArgumentException("Security key cannot be null or empty.", nameof(appSecurityKey));
            if (string.IsNullOrWhiteSpace(issuer))
                throw new ArgumentException("Issuer cannot be null or empty.", nameof(issuer));
            if (string.IsNullOrWhiteSpace(audience))
                throw new ArgumentException("Audience cannot be null or empty.", nameof(audience));
            
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSecurityKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claimsInfo = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, userRole.ToString())
            };
            
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claimsInfo,
                expires: DateTime.UtcNow.AddHours(expirationHours),
                signingCredentials: signingCredentials
            );
            
            var userToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            
            _logger.LogInformation("JWT token created for user {UserId} with {Hours}h expiration", 
                userId, expirationHours);
            
            return userToken;
        }
        
        public async Task<UserReadOnlyDto> SignUpUserAsync(UserSignupDto signupDto)
        {
            if (signupDto is null)
            {
                throw new InvalidArgumentException("User", "User signup data cannot be null.");
            }

            try
            {
                // Check for existing username
                var existingUserByUsername = await _unitOfWork.UserRepository.GetUserByUsernameAsync(signupDto.Username!);
                if (existingUserByUsername != null)
                {
                    _logger.LogWarning("Signup failed: Username {Username} already exists", signupDto.Username);
                    throw new EntityAlreadyExistsException("User", $"Username '{signupDto.Username}' is already taken.");
                }
                
                // Check for existing email
                var existingUserByEmail = await _unitOfWork.UserRepository.GetUserByEmailAsync(signupDto.Email!);
                if (existingUserByEmail != null)
                {
                    _logger.LogWarning("Signup failed: Email {Email} already exists", signupDto.Email);
                    throw new EntityAlreadyExistsException("User", $"Email '{signupDto.Email}' is already registered.");
                }
                
                // Map DTO to entity
                var newUser = _mapper.Map<User>(signupDto);

                // Encrypt password
                newUser.Password = EncryptionUtil.Encrypt(signupDto.Password!); 

                // Set default role
                newUser.UserRole = UserRole.User;

                // Save to database
                await _unitOfWork.UserRepository.AddAsync(newUser);
                await _unitOfWork.SaveAsync(); 

                _logger.LogInformation("New user created: {Username} (ID: {UserId})", newUser.Username, newUser.Id);

                return _mapper.Map<UserReadOnlyDto>(newUser);
            }
            catch (EntityAlreadyExistsException)
            {
                throw; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during user signup for {Username}", signupDto.Username);
                throw new ServerException("Server", "An unexpected error occurred during signup.");
            }
        }
    }
}