using System.ComponentModel.DataAnnotations;
using GoalTrackerApp.Core.Enums;

namespace GoalTrackerApp.Dto;

/// <summary>
/// Data transfer object for updating an existing goal.
/// </summary>
public class GoalUpdateDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }

    /// <summary>
    /// Optional due date for the goal.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Goal status (InProgress, Completed, Cancelled).
    /// Allows users to mark goals as completed.
    /// </summary>
    [EnumDataType(typeof(GoalStatus), ErrorMessage = "Invalid Status value.")]
    public GoalStatus Status { get; set; }
    public int? GoalCategoryId { get; set; }
    
}