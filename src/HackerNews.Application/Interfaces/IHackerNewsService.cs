using HackerNews.Application.Models;

namespace HackerNews.Application.Interfaces;

public interface IHackerNewsService
{
    Task<IReadOnlyList<Story>> GetBestStoriesAsync(int count, CancellationToken cancellationToken = default);
}
