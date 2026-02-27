using HackerNews.Api.DTOs;
using HackerNews.Api.Hubs;
using HackerNews.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace HackerNews.Api.Services;

public class StoryUpdateService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<StoriesHub> _hubContext;
    private readonly ILogger<StoryUpdateService> _logger;
    private List<StoryResponse> _lastStories = [];

    public StoryUpdateService(
        IServiceScopeFactory scopeFactory,
        IHubContext<StoriesHub> hubContext,
        ILogger<StoryUpdateService> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IHackerNewsService>();
                var stories = await service.GetBestStoriesAsync(20, stoppingToken);
                var responses = stories.Select(StoryResponse.FromStory).ToList();

                if (!ResponsesEqual(_lastStories, responses))
                {
                    _lastStories = responses;
                    await _hubContext.Clients.All.SendAsync("ReceiveStoryUpdates", responses, stoppingToken);
                    _logger.LogInformation("Sent {Count} story updates to connected clients", responses.Count);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Error fetching story updates");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private static bool ResponsesEqual(List<StoryResponse> a, List<StoryResponse> b)
    {
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
        {
            if (a[i].Score != b[i].Score || a[i].CommentCount != b[i].CommentCount || a[i].Title != b[i].Title)
                return false;
        }
        return true;
    }
}
