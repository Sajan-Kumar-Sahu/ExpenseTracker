namespace ExpenseTracker.Application.Common
{
    public class Result<T> : Result
    {
        public T? Data { get; private set; }

        private Result(bool isSuccess, string message, T? data)
            : base(isSuccess, message)
        {
            Data = data;
        }

        public static Result<T> Success(T data, string message = "") => new(true, message, data);
        public static new Result<T> Failure(string message) => new(false, message, default);
    }
}
