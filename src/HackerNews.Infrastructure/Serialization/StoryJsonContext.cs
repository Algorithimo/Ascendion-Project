using System.Text.Json.Serialization;
using HackerNews.Application.Models;

namespace HackerNews.Infrastructure.Serialization;

[JsonSerializable(typeof(Story))]
[JsonSerializable(typeof(int[]))]
[JsonSerializable(typeof(List<Story>))]
public partial class StoryJsonContext : JsonSerializerContext
{
}
