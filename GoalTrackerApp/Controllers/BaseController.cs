using Microsoft.AspNetCore.Mvc;
using GoalTrackerApp.Services;
using System.Security.Claims;
using GoalTrackerApp.Models;

namespace GoalTrackerApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
    protected readonly IApplicationService ApplicationService;

    public BaseController(IApplicationService applicationService)
    {
        ApplicationService = applicationService;
    }

    private ApplicationUser? _appUser;
        
    /// <summary>
    /// Gets the current authenticated user from JWT claims.
    /// Cached for the lifetime of the request.
    /// </summary>
    protected ApplicationUser? AppUser
    {
        get
        {
            // Return cached user if already resolved
            if (_appUser != null)
            {
                return _appUser;
            }

            // Check if user is authenticated
            if (User?.Claims == null || !User.Claims.Any())
            {
                return null;
            }

            // Validate required claim exists
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return null;
            }
                    
            // Build and cache ApplicationUser
            _appUser = new ApplicationUser
            {
                Id = userId,
                Username = User.FindFirst(ClaimTypes.Name)?.Value,
                Email = User.FindFirst(ClaimTypes.Email)?.Value
            };
                
            return _appUser;
        }
    }
}