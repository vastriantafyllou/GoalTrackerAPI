namespace GoalTrackerApp.Exceptions
{
    public class EntityNotAuthorizedException : AppException
    {
        private static readonly string DefaultCode = "NotAuthorized";

        public EntityNotAuthorizedException(string code, string message)
            : base(code + DefaultCode, message)
        {
        }
    }
}