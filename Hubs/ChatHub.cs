using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace O_market.Hubs
{
    public class ChatHub : Hub
    {

        [Authorize]
        // Join room for a specific Ad
        public async Task JoinAdRoom(int adId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"ad-{adId}");
        }

        // Leave room
        public async Task LeaveAdRoom(int adId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ad-{adId}");
        }


    }
}
