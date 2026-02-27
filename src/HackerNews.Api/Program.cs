using System.Reflection;
using System.Runtime;
using HackerNews.Api.Hubs;
using HackerNews.Api.Services;
using HackerNews.Infrastructure;

GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddInfrastructure();
builder.Services.AddSignalR();
builder.Services.AddHostedService<StoryUpdateService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Hacker News Best Stories API",
        Version = "v1",
        Description = "RESTful API that returns the best stories from Hacker News, ordered by score."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hacker News API v1");
});

app.UseHttpsRedirection();
app.MapControllers();
app.MapHub<StoriesHub>("/hubs/stories");

app.Run();

public partial class Program { }
