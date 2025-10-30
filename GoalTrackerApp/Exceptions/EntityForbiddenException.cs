namespace GoalTrackerApp.Exceptions;

    public class EntityForbiddenException : AppException    
    {
        private static readonly string DefaultCode = "Forbidden";

        public EntityForbiddenException(string code, string message) : base(code + DefaultCode, message) { }
        
    }
