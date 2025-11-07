namespace GoalTrackerApp.Dto
{
    public class GoalCategoryReadOnlyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
