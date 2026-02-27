using HackerNews.Application.Interfaces;
using HackerNews.Application.Models;
using HackerNews.Application.Services;
using Moq;

namespace HackerNews.Tests;

public class HackerNewsServiceTests
{
    private readonly Mock<IHackerNewsClient> _clientMock = new();
    private readonly HackerNewsService _sut;

    public HackerNewsServiceTests()
    {
        _sut = new HackerNewsService(_clientMock.Object);
    }

    [Fact]
    public async Task GetBestStoriesAsync_ReturnsStoriesOrderedByScoreDescending()
    {
        var ids = new[] { 1, 2, 3 };
        _clientMock.Setup(c => c.GetBestStoryIdsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ids);
        _clientMock.Setup(c => c.GetStoryByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Story { Id = 1, Score = 10, Title = "Low" });
        _clientMock.Setup(c => c.GetStoryByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Story { Id = 2, Score = 50, Title = "High" });
        _clientMock.Setup(c => c.GetStoryByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Story { Id = 3, Score = 30, Title = "Mid" });

        var result = await _sut.GetBestStoriesAsync(3);

        Assert.Equal(3, result.Count);
        Assert.Equal(50, result[0].Score);
        Assert.Equal(30, result[1].Score);
        Assert.Equal(10, result[2].Score);
    }

    [Fact]
    public async Task GetBestStoriesAsync_TakesOnlyRequestedCount()
    {
        var ids = new[] { 1, 2, 3 };
        _clientMock.Setup(c => c.GetBestStoryIdsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ids);
        _clientMock.Setup(c => c.GetStoryByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int id, CancellationToken _) => new Story { Id = id, Score = id * 10 });

        var result = await _sut.GetBestStoriesAsync(2);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetBestStoriesAsync_HandlesNullStories()
    {
        var ids = new[] { 1, 2 };
        _clientMock.Setup(c => c.GetBestStoryIdsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ids);
        _clientMock.Setup(c => c.GetStoryByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Story { Id = 1, Score = 10 });
        _clientMock.Setup(c => c.GetStoryByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Story?)null);

        var result = await _sut.GetBestStoriesAsync(5);

        Assert.Single(result);
        Assert.Equal(10, result[0].Score);
    }
}
