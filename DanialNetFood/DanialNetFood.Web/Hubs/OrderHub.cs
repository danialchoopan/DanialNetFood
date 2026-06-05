using Microsoft.AspNetCore.SignalR;

namespace DanialNetFood.Web.Hubs
{
    public class OrderHub : Hub
    {
        public async Task JoinOrderGroup(int orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Order_{orderId}");
        }

        public async Task UpdateOrderStatus(int orderId, string status)
        {
            await Clients.Group($"Order_{orderId}").SendAsync("ReceiveStatusUpdate", status);
        }
    }
}
