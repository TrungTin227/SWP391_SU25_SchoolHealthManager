namespace DTOs.Common
{
    public class BatchOperationResultDTO
    {
        public int TotalRequested { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> SuccessIds { get; set; } = new();
        public List<BatchOperationErrorDTO> Errors { get; set; } = new();
        public string Message { get; set; } = string.Empty;
        public bool IsPartialSuccess => SuccessCount > 0 && FailureCount > 0;
        public bool IsCompleteSuccess => SuccessCount == TotalRequested && FailureCount == 0;
        public bool IsCompleteFailure => SuccessCount == 0 && FailureCount > 0;
    }
}
