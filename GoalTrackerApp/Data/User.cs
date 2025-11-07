using GoalTrackerApp.Core.Enums;

namespace GoalTrackerApp.Data
{
    public class User : BaseEntity
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Firstname { get; set; } = null!;
        public string Lastname { get; set; } = null!;
        public UserRole UserRole { get; set; }

        public virtual ICollection<Goal> Goals { get; set; } = new HashSet<Goal>();
        public virtual ICollection<GoalCategory> GoalCategories { get; set; } = new HashSet<GoalCategory>();
    }
}