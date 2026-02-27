using HackerNews.Application.Interfaces;
using HackerNews.Application.Services;
using HackerNews.Infrastructure.Caching;
using HackerNews.Infrastructure.Clients;
using Microsoft.Extensions.DependencyInjection;

namespace HackerNews.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string redisConnection = "localhost:6379")
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "HackerNews:";
        });

        services.AddHttpClient<HackerNewsClient>(client =>
        {
            client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/");
        })
        .AddStandardResilienceHandler();

        services.AddSingleton<IHackerNewsClient>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient(nameof(HackerNewsClient));
            var inner = new HackerNewsClient(httpClient);
            var cache = sp.GetRequiredService<Microsoft.Extensions.Caching.Distributed.IDistributedCache>();
            return new CachedHackerNewsClient(inner, cache);
        });

        services.AddScoped<IHackerNewsService, HackerNewsService>();

        return services;
    }
}
