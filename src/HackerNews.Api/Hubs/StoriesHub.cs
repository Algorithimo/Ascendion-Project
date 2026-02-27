using Microsoft.AspNetCore.SignalR;

namespace HackerNews.Api.Hubs;

public class StoriesHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
}
