namespace GoalTrackerApp.Exceptions
{
    public class InvalidArgumentException : AppException
    {
        private static readonly string DefaultCode = "InvalidArgument";

        public InvalidArgumentException(string code, string message)
            : base(code + DefaultCode, message)
        {
        }
    }
}
