namespace HelpdeskSaaS.Services
{
    public interface INotificationService
    {
        Task SendToUserAsync(int userId, string message);

        Task SendToUsersAsync(
            IEnumerable<int> userIds,
            string message);
    }
}
