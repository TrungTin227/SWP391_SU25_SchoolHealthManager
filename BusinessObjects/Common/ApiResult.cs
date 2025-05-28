namespace BusinessObjects.Common
{
    public class ApiResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public static ApiResult<T> Success(T data) =>
            new ApiResult<T> { IsSuccess = true, Data = data };
        public static ApiResult<T> Failure(string error) =>
            new ApiResult<T> { IsSuccess = false, Message = error };
    }
    public class ApiResult
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }

        public static ApiResult Success() => new ApiResult { IsSuccess = true };
        public static ApiResult Failure(string error) => new ApiResult { IsSuccess = false, Message = error };
    }
}
