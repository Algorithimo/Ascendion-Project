using HackerNews.Application.Interfaces;
using HackerNews.Application.Models;

namespace HackerNews.Application.Services;

public class HackerNewsService : IHackerNewsService
{
    private readonly IHackerNewsClient _client;
    private static readonly SemaphoreSlim _throttle = new(20);

    public HackerNewsService(IHackerNewsClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<Story>> GetBestStoriesAsync(int count, CancellationToken cancellationToken = default)
    {
        var ids = await _client.GetBestStoryIdsAsync(cancellationToken);

        var tasks = ids.Select(id => FetchStoryThrottledAsync(id, cancellationToken));
        var stories = await Task.WhenAll(tasks);

        return stories
            .Where(s => s is not null)
            .OrderByDescending(s => s!.Score)
            .Take(count)
            .ToList()!;
    }

    private async Task<Story?> FetchStoryThrottledAsync(int id, CancellationToken cancellationToken)
    {
        await _throttle.WaitAsync(cancellationToken);
        try
        {
            return await _client.GetStoryByIdAsync(id, cancellationToken);
        }
        finally
        {
            _throttle.Release();
        }
    }
}
