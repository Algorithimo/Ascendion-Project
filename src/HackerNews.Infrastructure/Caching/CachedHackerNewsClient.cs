using System.Text.Json;
using HackerNews.Application.Interfaces;
using HackerNews.Application.Models;
using HackerNews.Infrastructure.Serialization;
using Microsoft.Extensions.Caching.Distributed;

namespace HackerNews.Infrastructure.Caching;

public class CachedHackerNewsClient : IHackerNewsClient
{
    private readonly IHackerNewsClient _inner;
    private readonly IDistributedCache _cache;
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    private const string BestStoryIdsCacheKey = "best-story-ids";

    public CachedHackerNewsClient(IHackerNewsClient inner, IDistributedCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<IReadOnlyList<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _cache.GetAsync(BestStoryIdsCacheKey, cancellationToken);
        if (cached is not null)
        {
            return JsonSerializer.Deserialize(cached, StoryJsonContext.Default.Int32Array) ?? [];
        }

        var ids = await _inner.GetBestStoryIdsAsync(cancellationToken);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(ids.ToArray(), StoryJsonContext.Default.Int32Array);
        await _cache.SetAsync(BestStoryIdsCacheKey, bytes, CacheOptions, cancellationToken);
        return ids;
    }

    public async Task<Story?> GetStoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"story-{id}";
        var cached = await _cache.GetAsync(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return JsonSerializer.Deserialize(cached, StoryJsonContext.Default.Story);
        }

        var story = await _inner.GetStoryByIdAsync(id, cancellationToken);
        if (story is not null)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(story, StoryJsonContext.Default.Story);
            await _cache.SetAsync(cacheKey, bytes, CacheOptions, cancellationToken);
        }
        return story;
    }
}
