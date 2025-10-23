using GoalTrackerApp.Core.Enums;

namespace GoalTrackerApp.Data 
{
    public class Goal : BaseEntity 
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; } 
        public GoalStatus GoalStatus { get; set; } = GoalStatus.InProgress;
        public DateTime? DueDate { get; set; } 
        
        // User - Goals (One-to-Many)
        public int UserId { get; set; }  // foreign key
        public virtual User User { get; set; } = null!; 
        
        // GoalCategory - Goal (One-to-Many, Optional)
        public int? GoalCategoryId { get; set; } // foreign key
        public virtual GoalCategory? GoalCategory { get; set; } 
    }
}

    