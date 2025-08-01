namespace Services.Interfaces
{
    public interface ICurrentUserService
    {
        public Guid? GetUserId();
        public bool IsAdmin();
        string? GetUserFullName();

    }
}
