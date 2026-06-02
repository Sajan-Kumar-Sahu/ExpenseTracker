namespace ExpenseTracker.Application.Common
{
    public class Result
    {
        public bool IsSuccess { get; private set; }
        public string Message { get; private set; } = string.Empty;

        protected Result(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public static Result Success(string message = "") => new(true, message);
        public static Result Failure(string message) => new(false, message);
    }
}
