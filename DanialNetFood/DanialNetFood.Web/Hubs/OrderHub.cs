using Microsoft.AspNetCore.SignalR;

namespace DanialNetFood.Web.Hubs
{
    public class OrderHub : Hub
    {
        public async Task JoinOrderGroup(string orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Order_{orderId}");
        }

        public async Task JoinRestaurantGroup(string restaurantId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Restaurant_{restaurantId}");
        }

        public async Task JoinDriverGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Drivers");
        }

        public async Task UpdateOrderStatus(string orderId, string status)
        {
            await Clients.Group($"Order_{orderId}").SendAsync("ReceiveStatusUpdate", orderId, status);
        }

        public async Task NotifyNewOrder(string restaurantId, object order)
        {
            await Clients.Group($"Restaurant_{restaurantId}").SendAsync("ReceiveNewOrder", order);
        }

        public async Task NotifyDriversOfNewJob(object order)
        {
            await Clients.Group("Drivers").SendAsync("ReceiveNewJob", order);
        }
    }
}
