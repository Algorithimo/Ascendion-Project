using HackerNews.Api.DTOs;
using HackerNews.Application.Models;

namespace HackerNews.Tests;

public class StoryResponseTests
{
    [Fact]
    public void FromStory_MapsFieldsCorrectly()
    {
        var story = new Story
        {
            Id = 1,
            Title = "Test Story",
            Url = "https://example.com",
            By = "testuser",
            Time = 1700000000,
            Score = 42,
            Descendants = 10
        };

        var response = StoryResponse.FromStory(story);

        Assert.Equal("Test Story", response.Title);
        Assert.Equal("https://example.com", response.Uri);
        Assert.Equal("testuser", response.PostedBy);
        Assert.Equal(42, response.Score);
        Assert.Equal(10, response.CommentCount);
    }

    [Fact]
    public void FromStory_ConvertsUnixTimeToDateTimeOffset()
    {
        var story = new Story { Time = 1700000000 };

        var response = StoryResponse.FromStory(story);

        var expected = DateTimeOffset.FromUnixTimeSeconds(1700000000);
        Assert.Equal(expected, response.Time);
    }

    [Fact]
    public void FromStory_HandlesNullUrl()
    {
        var story = new Story { Url = null };

        var response = StoryResponse.FromStory(story);

        Assert.Null(response.Uri);
    }
}
