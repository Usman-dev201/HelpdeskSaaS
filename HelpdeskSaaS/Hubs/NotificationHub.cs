using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HelpdeskSaaS.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("====================================");
            Console.WriteLine("🔥🔥🔥 NOTIFICATION HUB CONNECTED 🔥🔥🔥");
            Console.WriteLine($"ConnectionId: {Context.ConnectionId}");
            Console.WriteLine($"UserIdentifier: {Context.UserIdentifier}");

            var nameIdentifier = Context.User?.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier
            )?.Value;

            Console.WriteLine($"NameIdentifier Claim: {nameIdentifier}");
            Console.WriteLine("====================================");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine("====================================");
            Console.WriteLine("🔴 NOTIFICATION HUB DISCONNECTED");
            Console.WriteLine($"ConnectionId: {Context.ConnectionId}");
            Console.WriteLine($"UserIdentifier: {Context.UserIdentifier}");
            Console.WriteLine("====================================");

            await base.OnDisconnectedAsync(exception);
        }
    }
}
