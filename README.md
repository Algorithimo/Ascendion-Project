# Hacker News Best Stories API

RESTful API built with **ASP.NET Core 9** that retrieves the best *n* stories from [Hacker News](https://news.ycombinator.com/), sorted by score descending.

---

## How to Run

### Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) | 9.x | Build and run the API |
| [Docker](https://docs.docker.com/get-docker/) | 20.x+ | Run Redis container |

### Step 1 — Start Redis

```bash
docker compose up -d
```

Verify it's running:

```bash
docker ps
# Expected: redis:alpine container on port 6379
```

### Step 2 — Build and Run

```bash
# Build the solution
dotnet build HackerNews.sln

# Start the API
dotnet run --project src/HackerNews.Api --urls "http://localhost:5123"
```

### Step 3 — Use the Application

| URL | Description |
|-----|-------------|
| http://localhost:5123/ | Frontend — interactive Hacker News viewer with live updates |
| http://localhost:5123/swagger | Swagger UI — interactive API documentation |
| http://localhost:5123/api/stories?count=5 | REST API — returns top 5 stories as JSON |

### Step 4 — Run Tests

**Unit tests** (13 tests — xUnit + Moq):

```bash
dotnet test HackerNews.sln --verbosity normal
```

**Load tests** (requires [Bombardier](https://github.com/codesenberg/bombardier)):

```bash
bash loadtest.sh http://localhost:5123
```

---

## API Reference

### `GET /api/stories`

Returns the top *n* best stories from Hacker News, ordered by score descending.

**Parameters:**

| Parameter | Type | Default | Range | Description |
|-----------|------|---------|-------|-------------|
| `count` | int | 10 | 1–200 | Number of stories to return |

**Success Response (200):**

```json
[
  {
    "title": "A uBlock Origin update was rejected from the Chrome Web Store",
    "uri": "https://github.com/AnotherRepo",
    "postedBy": "nickthegreek",
    "time": "2023-11-14T18:13:20+00:00",
    "score": 1756,
    "commentCount": 572
  }
]
```

**Error Response (400):** Returned when `count` is outside the 1–200 range.

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Invalid parameter",
  "status": 400,
  "detail": "The count parameter must be between 1 and 200."
}
```

---

## Assumptions

1. **Hacker News API availability** — The upstream API at `https://hacker-news.firebaseio.com/v0/` is publicly accessible and does not require authentication. If it becomes unavailable, the API relies on Polly retry/circuit-breaker policies to handle transient failures gracefully.

2. **Redis on localhost** — Redis is expected to run on `localhost:6379` (the default). This is configurable via the `AddInfrastructure()` method in `DependencyInjection.cs`. In production, this would be injected via `appsettings.json` or environment variables.

3. **5-minute cache TTL** — All cached data (story IDs and individual stories) expires after 5 minutes. This was chosen as a balance between data freshness and reducing load on the Hacker News API. Stories that change frequently (scores, comment counts) are also updated in real-time via the SignalR background service every 30 seconds for connected clients.

4. **200-story upper bound** — The maximum `count` is capped at 200 per request. The Hacker News `/beststories` endpoint returns up to 200 IDs, so this matches the upstream limit. Requesting more would require pagination or a different approach.

5. **Single-instance deployment** — The current SignalR setup uses in-memory backplane, which works for a single server. Multi-instance deployments would require a Redis backplane for SignalR.

6. **Concurrent request throttling** — A `SemaphoreSlim(20)` limits parallel outbound requests to the Hacker News API to 20 at a time. This prevents socket exhaustion and respects the upstream API's implicit rate limits without formal documentation on their thresholds.

---

## Enhancements and Changes Given More Time

### High Priority

- **Rate limiting** — Add ASP.NET Core rate limiting middleware (`AddRateLimiter`) to protect the API from abuse. A sliding window of ~100 requests/minute per IP would be reasonable.

- **Health checks** — Implement `/health` endpoint using `Microsoft.Extensions.Diagnostics.HealthChecks` to monitor Redis connectivity and Hacker News API reachability. Essential for production monitoring and container orchestration (Kubernetes liveness/readiness probes).

- **Response compression** — Enable gzip/brotli compression middleware for JSON responses. With `count=200`, payloads can be several KB; compression would reduce bandwidth by ~70%.

- **Integration tests** — Add `WebApplicationFactory<Program>` integration tests that spin up the full pipeline (with a mocked `IHackerNewsClient`) to test the HTTP layer end-to-end without relying on external services.

### Medium Priority

- **Redis backplane for SignalR** — Use `AddStackExchangeRedis()` on the SignalR builder so real-time updates work across multiple API instances behind a load balancer.

- **Structured logging** — Replace default logging with Serilog + structured sinks (Seq, ELK, or Application Insights). Add correlation IDs to trace requests across the cache and HTTP layers.

- **Configuration externalization** — Move hardcoded values (cache TTL, concurrency limit, polling interval) to `appsettings.json` with `IOptions<T>` pattern so they can be tuned per environment without redeployment.

- **ETag/conditional requests** — Return `ETag` headers based on cache state so clients can use `If-None-Match` to avoid downloading unchanged data (304 Not Modified).

### Lower Priority

- **Pagination** — Support `offset`/`limit` or cursor-based pagination for clients that want to browse beyond the top 200.

- **Docker multi-stage build** — Add a `Dockerfile` with multi-stage build (SDK for build, runtime for final image) so the entire application can be deployed as `docker compose up` without needing .NET SDK installed.

- **CI/CD pipeline** — Add GitHub Actions workflow to build, test, and optionally publish a Docker image on each push.

- **API versioning** — Implement URL or header-based versioning (`/api/v1/stories`) to allow non-breaking API evolution.

---

## Architecture

Clean Architecture with 3 layers:

```
HackerNews.Api              → Controllers, DTOs, SignalR Hub, Frontend
HackerNews.Application      → Interfaces, domain models, business logic
HackerNews.Infrastructure   → HTTP client, Redis cache, Polly resilience, DI
```

### Key Design Decisions

| Pattern | Implementation | Why |
|---------|---------------|-----|
| **Decorator** | `CachedHackerNewsClient` wraps `HackerNewsClient` | Transparent caching without modifying the HTTP client |
| **Distributed Cache** | Redis via `IDistributedCache` | Shared across instances, survives restarts, 5-min TTL |
| **Source Generators** | `StoryJsonContext` (System.Text.Json) | Zero-reflection serialization, reduced GC pressure |
| **Throttling** | `SemaphoreSlim(20)` | Prevents socket exhaustion on concurrent HN API calls |
| **Resilience** | Polly (3x exponential retry + circuit breaker) | Graceful degradation on upstream failures |
| **Real-time** | SignalR Hub + BackgroundService (30s poll) | Live score/comment updates via WebSocket |
| **Low Latency GC** | `GCSettings.LatencyMode = SustainedLowLatency` | Minimizes GC pauses for consistent response times |

---

## Project Structure

```
src/
  HackerNews.Api/
    Controllers/StoriesController.cs     # REST endpoint with validation
    DTOs/StoryResponse.cs                # Response DTO with field mapping
    Hubs/StoriesHub.cs                   # SignalR hub for real-time updates
    Services/StoryUpdateService.cs       # Background service (30s polling)
    wwwroot/index.html                   # Frontend SPA
    Program.cs                           # DI, middleware, GC configuration
  HackerNews.Application/
    Models/Story.cs                      # Domain model
    Interfaces/                          # IHackerNewsClient, IHackerNewsService
    Services/HackerNewsService.cs        # Business logic + throttling
  HackerNews.Infrastructure/
    Clients/HackerNewsClient.cs          # Typed HttpClient for HN API
    Caching/CachedHackerNewsClient.cs    # Redis cache decorator (5-min TTL)
    Serialization/StoryJsonContext.cs     # JSON source generator context
    DependencyInjection.cs               # Service registration
tests/
  HackerNews.Tests/                      # 13 unit tests (xUnit + Moq)
doc/
  Backend_Developer_Coding_Test.pdf      # Original challenge specification
docker-compose.yml                       # Redis container
loadtest.sh                              # Bombardier load test script
HackerNews.sln                           # Visual Studio solution
```
