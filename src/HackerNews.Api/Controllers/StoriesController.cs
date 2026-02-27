using HackerNews.Api.DTOs;
using HackerNews.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HackerNews.Api.Controllers;

/// <summary>
/// Provides access to Hacker News best stories.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StoriesController : ControllerBase
{
    private readonly IHackerNewsService _service;

    public StoriesController(IHackerNewsService service)
    {
        _service = service;
    }

    /// <summary>
    /// Returns the top n best stories from Hacker News, ordered by score descending.
    /// </summary>
    /// <param name="count">Number of stories to return (1-200).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of best stories.</returns>
    /// <response code="200">Returns the list of best stories.</response>
    /// <response code="400">If count is out of range (1-200).</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBestStories([FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        if (count < 1 || count > 200)
        {
            return Problem(
                detail: "The count parameter must be between 1 and 200.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid parameter");
        }

        var stories = await _service.GetBestStoriesAsync(count, cancellationToken);
        var response = stories.Select(StoryResponse.FromStory);
        return Ok(response);
    }
}
