namespace UserDTOs.DTOs.Response
{
    public class ErrorResponse
    {
        public string Title{ get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
