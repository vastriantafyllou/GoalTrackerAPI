namespace GoalTrackerApp.Exceptions
{
    public class EntityNotFoundException : AppException
    {
        private static readonly string DefaultCode = "NotFound";

        public EntityNotFoundException(string code, string message)
            : base(code + DefaultCode, message)
        {
        }
    }
}
