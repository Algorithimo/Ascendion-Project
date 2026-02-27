using HackerNews.Application.Models;

namespace HackerNews.Api.DTOs;

public class StoryResponse
{
    public string Title { get; set; } = string.Empty;
    public string? Uri { get; set; }
    public string PostedBy { get; set; } = string.Empty;
    public DateTimeOffset Time { get; set; }
    public int Score { get; set; }
    public int CommentCount { get; set; }

    public static StoryResponse FromStory(Story story)
    {
        return new StoryResponse
        {
            Title = story.Title,
            Uri = story.Url,
            PostedBy = story.By,
            Time = DateTimeOffset.FromUnixTimeSeconds(story.Time),
            Score = story.Score,
            CommentCount = story.Descendants
        };
    }
}
