using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using GoalTrackerApp.Core.Filters;
using GoalTrackerApp.Data;
using GoalTrackerApp.Dto;
using GoalTrackerApp.Models;
using GoalTrackerApp.Repositories;
using Serilog;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using GoalTrackerApp.Core.Enums;
using GoalTrackerApp.Exceptions;
using GoalTrackerApp.Core.Security;
using Microsoft.IdentityModel.Tokens;

namespace GoalTrackerApp.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger = 
            new LoggerFactory().AddSerilog().CreateLogger<UserService>();

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this._unitOfWork = unitOfWork;
            this._mapper = mapper;
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
                User? user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    throw new EntityNotFoundException("User", $"User with username: {username} not found");
                }
                
                _logger.LogInformation("User found: {Username}", username);
                return new UserReadOnlyDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Firstname = user.Firstname,
                    Lastname = user.Lastname,
                    UserRole = user.UserRole.ToString()
                };
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogError("Error retrieving user by username: {Username}. {Message}", username, ex.Message);
                throw;
            }
        }

        public async Task<User?> VerifyAndGetUserAsync(UserLoginDto credentials)
        {
            User? user = null;
            try
            {
                user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(credentials.Username!);

                if (user == null)
                    throw new EntityNotAuthorizedException("User", "Username does not exist");

                if (!EncryptionUtil.IsValidPassword(credentials.Password!, user.Password))
                    throw new EntityNotAuthorizedException("User", "Wrong password");
                
                _logger.LogInformation("User with username {Username} found", credentials.Username!);
            }
            catch (EntityNotAuthorizedException e)
            {
                _logger.LogError("Authentication failed for username {Username}. {Message}",
                    credentials.Username, e.Message);
                throw;
            }
            return user;
        }
        
        public async Task<UserReadOnlyDto> GetUserByIdAsync(int id)
        {
            User? user = null;

            try
            {
                user = await _unitOfWork.UserRepository.GetAsync(id);
                if (user == null)
                {
                    throw new EntityNotFoundException("User", "User with id: " + id + " not found");
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
        
        public string CreateUserToken(int userId, string username, string email, UserRole userRole, string appSecurityKey)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(appSecurityKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claimsInfo = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, userRole.ToString())
            };
            
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: "https://localhost:5001",
                audience: "https://localhost:5001",
                claims: claimsInfo,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: signingCredentials
            );
            
            // Serialize the token to a string
            var userToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            
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
                var existingUserByUsername = await _unitOfWork.UserRepository.GetUserByUsernameAsync(signupDto.Username!);
                if (existingUserByUsername != null)
                {
                    _logger.LogWarning("Signup failed: Username {Username} already exists", signupDto.Username);
                    throw new EntityAlreadyExistsException("User", "Username '" + signupDto.Username + "' already exists.");
                }
                var existingUserByEmail = await _unitOfWork.UserRepository.GetUserByEmailAsync(signupDto.Email!);
                if (existingUserByEmail != null)
                {
                    _logger.LogWarning("Signup failed: Email {Email} already exists", signupDto.Email);
                    throw new EntityAlreadyExistsException("User", "Email '" + signupDto.Email + "' already exists.");
                }
                
                var newUser = _mapper.Map<User>(signupDto);

                newUser.Password = EncryptionUtil.Encrypt(signupDto.Password!); 

                newUser.UserRole = UserRole.User;

                await _unitOfWork.UserRepository.AddAsync(newUser);
                await _unitOfWork.SaveAsync(); 

                _logger.LogInformation("New user created with username: {Username}", newUser.Username);

                return _mapper.Map<UserReadOnlyDto>(newUser);
            }
            catch (EntityAlreadyExistsException)
            {
                throw; 
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An unexpected error occurred during user signup for {Username}", signupDto.Username);
                
                throw new ServerException("Server", "An unexpected error occurred during signup.");
            }
        }
        
        public async Task<UserReadOnlyDto> UpdateUserAsync(int userId, UserUpdateDto updateDto)
        {
            if (updateDto is null)
            {
                throw new InvalidArgumentException("User", "User update data cannot be null.");
            }

            try
            {
                var user = await _unitOfWork.UserRepository.GetAsync(userId);
                if (user == null)
                {
                    throw new EntityNotFoundException("User", $"User with ID: {userId} not found.");
                }

                if (!string.IsNullOrEmpty(updateDto.Username))
                {
                    var existingUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(updateDto.Username);
                    if (existingUser != null && existingUser.Id != userId)
                    {
                        throw new EntityAlreadyExistsException("User", $"Username '{updateDto.Username}' already exists.");
                    }
                    user.Username = updateDto.Username;
                }

                if (!string.IsNullOrEmpty(updateDto.Email))
                {
                    var existingUser = await _unitOfWork.UserRepository.GetUserByEmailAsync(updateDto.Email);
                    if (existingUser != null && existingUser.Id != userId)
                    {
                        throw new EntityAlreadyExistsException("User", $"Email '{updateDto.Email}' already exists.");
                    }
                    user.Email = updateDto.Email;
                }

                if (!string.IsNullOrEmpty(updateDto.Firstname))
                {
                    user.Firstname = updateDto.Firstname;
                }

                if (!string.IsNullOrEmpty(updateDto.Lastname))
                {
                    user.Lastname = updateDto.Lastname;
                }

                if (updateDto.UserRole.HasValue)
                {
                    user.UserRole = updateDto.UserRole.Value;
                }

                await _unitOfWork.UserRepository.UpdateAsync(user);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("User with ID: {UserId} updated successfully", userId);

                return _mapper.Map<UserReadOnlyDto>(user);
            }
            catch (EntityAlreadyExistsException)
            {
                throw;
            }
            catch (EntityNotFoundException)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An unexpected error occurred during user update for ID: {UserId}", userId);
                throw new ServerException("Server", "An unexpected error occurred during user update.");
            }
        }

        public async Task DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetAsync(userId);
                if (user == null)
                {
                    throw new EntityNotFoundException("User", $"User with ID: {userId} not found.");
                }

                await _unitOfWork.UserRepository.DeleteAsync(userId);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("User with ID: {UserId} deleted successfully", userId);
            }
            catch (EntityNotFoundException)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An unexpected error occurred during user deletion for ID: {UserId}", userId);
                throw new ServerException("Server", "An unexpected error occurred during user deletion.");
            }
        }
    }
}