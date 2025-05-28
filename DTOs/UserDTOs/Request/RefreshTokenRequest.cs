namespace DTOs.UserDTOs.Request
{
    public class RefreshTokenRequest
    {
        public Guid Id { get; set; } 

        public string RefreshToken { get; set; } = string.Empty;
    }
}
