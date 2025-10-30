namespace GoalTrackerApp.Exceptions
{
    public class EntityAlreadyExistsException : AppException
    {
        private static readonly string DefaultCode = "AlreadyExists";

        public EntityAlreadyExistsException(string code, string message)
            : base(code + DefaultCode, message)
        {
        }

        
        
        
    }
}
