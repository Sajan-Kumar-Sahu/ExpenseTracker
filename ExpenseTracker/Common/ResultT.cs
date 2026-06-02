namespace ExpenseTracker.API.Common
{
    public class Result<T> : Result
    {
        public T? Data { get; set; }
    }
}
