using System.Net.Http.Json;
using HackerNews.Application.Interfaces;
using HackerNews.Application.Models;
using HackerNews.Infrastructure.Serialization;

namespace HackerNews.Infrastructure.Clients;

public class HackerNewsClient : IHackerNewsClient
{
    private readonly HttpClient _httpClient;

    public HackerNewsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken = default)
    {
        var ids = await _httpClient.GetFromJsonAsync("beststories.json", StoryJsonContext.Default.Int32Array, cancellationToken);
        return ids ?? [];
    }

    public async Task<Story?> GetStoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync($"item/{id}.json", StoryJsonContext.Default.Story, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
