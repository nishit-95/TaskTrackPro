using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace MyApp.Core.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string userId, string message)
        {
            // Send the notification to the user with the specified userId
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }

        // Optional: Override OnConnectedAsync to handle client connections
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.Identity?.Name; // Or get the user ID from claims
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnConnectedAsync();
        }

        // Optional: Override OnDisconnectedAsync to handle client disconnections
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // You can add logic here when a client disconnects
            await base.OnDisconnectedAsync(exception);
        }
    }
}