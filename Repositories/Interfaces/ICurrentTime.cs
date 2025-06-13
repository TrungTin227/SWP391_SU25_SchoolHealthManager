namespace Repositories.Interfaces
{
    public interface ICurrentTime
    {
        DateTime GetCurrentTime();
        DateTime GetVietnamTime(); // Giờ Việt Nam (UTC+7)
    }
}
