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
        
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!; 
        
        public int? GoalCategoryId { get; set; }
        public virtual GoalCategory? GoalCategory { get; set; } 
    }
}

    