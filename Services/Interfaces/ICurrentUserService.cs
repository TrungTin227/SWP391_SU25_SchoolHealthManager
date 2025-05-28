namespace Services.Interfaces
{
    public interface ICurrentUserService
    {
        public string? GetUserId();
        public bool IsAdmin();

    }
}
