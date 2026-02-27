using HackerNews.Application.Models;

namespace HackerNews.Application.Interfaces;

public interface IHackerNewsClient
{
    Task<IReadOnlyList<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken = default);
    Task<Story?> GetStoryByIdAsync(int id, CancellationToken cancellationToken = default);
}
