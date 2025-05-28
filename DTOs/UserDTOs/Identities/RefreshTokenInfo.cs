namespace DTOs.UserDTOs.Identities
{
    public class RefreshTokenInfo
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiry { get; set; }
    }

}
