namespace BusinessObjects.Common
{
    public class ApiResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public Exception? Exception { get; set; }
        public static ApiResult<T> Success(T data, string message) =>
            new ApiResult<T> { IsSuccess = true, Data = data, Message = message };
        public static ApiResult<T> Failure(Exception error) =>
            new ApiResult<T> { IsSuccess = false, Message = error.Message };
        public static ApiResult<T> Error(T? data, Exception error) =>
           new ApiResult<T> { IsSuccess = false, Data = data, Message = error.Message };

    }
}
