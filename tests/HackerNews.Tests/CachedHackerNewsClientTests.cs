using HackerNews.Application.Interfaces;
using HackerNews.Application.Models;
using HackerNews.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;

namespace HackerNews.Tests;

public class CachedHackerNewsClientTests
{
    private readonly Mock<IHackerNewsClient> _innerMock = new();
    private readonly IDistributedCache _cache;
    private readonly CachedHackerNewsClient _sut;

    public CachedHackerNewsClientTests()
    {
        _cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        _sut = new CachedHackerNewsClient(_innerMock.Object, _cache);
    }

    [Fact]
    public async Task GetBestStoryIdsAsync_CachesResult_InnerCalledOnce()
    {
        var ids = new[] { 1, 2, 3 };
        _innerMock.Setup(c => c.GetBestStoryIdsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ids);

        var result1 = await _sut.GetBestStoryIdsAsync();
        var result2 = await _sut.GetBestStoryIdsAsync();

        Assert.Equal(ids, result1);
        Assert.Equal(ids, result2);
        _innerMock.Verify(c => c.GetBestStoryIdsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStoryByIdAsync_CachesPerStoryId()
    {
        var story1 = new Story { Id = 1, Title = "Story 1" };
        var story2 = new Story { Id = 2, Title = "Story 2" };

        _innerMock.Setup(c => c.GetStoryByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(story1);
        _innerMock.Setup(c => c.GetStoryByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(story2);

        var result1a = await _sut.GetStoryByIdAsync(1);
        var result1b = await _sut.GetStoryByIdAsync(1);
        var result2 = await _sut.GetStoryByIdAsync(2);

        Assert.Equal("Story 1", result1a!.Title);
        Assert.Equal("Story 1", result1b!.Title);
        Assert.Equal("Story 2", result2!.Title);
        _innerMock.Verify(c => c.GetStoryByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        _innerMock.Verify(c => c.GetStoryByIdAsync(2, It.IsAny<CancellationToken>()), Times.Once);
    }
}
