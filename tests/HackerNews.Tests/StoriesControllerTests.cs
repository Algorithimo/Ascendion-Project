using HackerNews.Api.Controllers;
using HackerNews.Api.DTOs;
using HackerNews.Application.Interfaces;
using HackerNews.Application.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HackerNews.Tests;

public class StoriesControllerTests
{
    private readonly Mock<IHackerNewsService> _serviceMock = new();
    private readonly StoriesController _sut;

    public StoriesControllerTests()
    {
        _sut = new StoriesController(_serviceMock.Object);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(201)]
    public async Task GetBestStories_InvalidCount_ReturnsBadRequest(int count)
    {
        var result = await _sut.GetBestStories(count);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetBestStories_ValidCount_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetBestStoriesAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Story>
            {
                new() { Id = 1, Title = "Test", Score = 100, By = "user", Time = 1700000000, Url = "https://example.com", Descendants = 5 }
            });

        var result = await _sut.GetBestStories(5);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var stories = Assert.IsAssignableFrom<IEnumerable<StoryResponse>>(okResult.Value);
        Assert.Single(stories);
    }

    [Fact]
    public async Task GetBestStories_DefaultCount_Uses10()
    {
        _serviceMock.Setup(s => s.GetBestStoriesAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Story>());

        var result = await _sut.GetBestStories();

        var okResult = Assert.IsType<OkObjectResult>(result);
        _serviceMock.Verify(s => s.GetBestStoriesAsync(10, It.IsAny<CancellationToken>()), Times.Once);
    }
}
