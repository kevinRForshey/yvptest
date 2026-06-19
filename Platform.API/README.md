# YouVersion.Platform.API

Typed HTTP client SDK for the [YouVersion Platform REST API](https://developers.youversion.com).

## What this package provides

- `IBibleClient` for Bible discovery and version metadata.
- `IPassageClient` for passage text/HTML retrieval.
- `IHighlightClient` for highlight read/write operations. (not fully implemented)
- `IYouVersionOAuthClient` for authorization-code + PKCE OAuth flow.
- `ITokenProvider` (default: `InMemoryTokenProvider`) for token persistence.
- Built-in `HttpClient` resilience and outbound rate limiting.
- DI extensions: `AddYouVersionApiClients(...)` and `AddYouVersionOAuth(...)`.

## Target framework

- `net10.0`

## Installation

Add package references as needed:

```xml
<ItemGroup>
  <PackageReference Include="YouVersion.Platform.API" Version="1.0.0" />
</ItemGroup>
```

## Minimal setup (no sign-in required)

For read-only operations (versions, books, passages), only an app key is required.

```csharp
builder.Services.AddYouVersionApiClients(options =>
{
    options.AppKey = builder.Configuration["YouVersionApi:AppKey"]!;
});
```

Example `appsettings.json`:

```json
{
  "YouVersionApi": {
    "AppKey": "YOUR_APP_KEY",
    "BaseAddress": "https://api.youversion.com",
    "Timeout": "00:00:30",
    "OutboundRequestsPerSecond": 10,
    "OutboundBurstSize": 20,
    "OutboundQueueLimit": 100
  }
}
```

## OAuth setup (optional)

Use this only when you need user-scoped operations (for example, highlight writes).

```csharp
builder.Services
    .AddYouVersionApiClients(builder.Configuration)
    .AddYouVersionOAuth(options =>
    {
        options.ClientId = builder.Configuration["YouVersionOAuth:ClientId"]!;
        options.RedirectUri = new Uri(builder.Configuration["YouVersionOAuth:RedirectUri"]!);
        options.Scopes = "passages highlights";
    });
```

> `AddYouVersionApiClients(...)` must be called before `AddYouVersionOAuth(...)`.

## Common usage examples

### List available Bible versions

```csharp
public sealed class VersionReader(IBibleClient bibleClient)
{
    public Task<PagedResult<BibleVersionSummary>> GetEnglishVersionsAsync(CancellationToken ct)
        => bibleClient.GetVersionsAsync(languageRange: "en", cancellationToken: ct);
}
```

### Fetch passage text

```csharp
public sealed class PassageReader(IPassageClient passageClient)
{
    public async Task<string> GetJohn316Async(CancellationToken ct)
    {
        var passage = await passageClient.GetPassageAsync(3034, "JHN.3.16", cancellationToken: ct);
        return passage.Content;
    }
}
```

### Create a highlight (OAuth required)

```csharp
public sealed class HighlightWriter(IHighlightClient highlightClient)
{
    public Task<Highlight> HighlightVerseAsync(CancellationToken ct)
        => highlightClient.CreateHighlightAsync(3034, "JHN.3.16", HighlightColor.Yellow, ct);
}
```

## Resilience and outbound rate limiting

The SDK configures `HttpClient` with:

- standard resilience handler (retry/backoff/timeout strategy), and
- local outbound token-bucket rate limiting per typed client.

Rate limit knobs (`YouVersionApi` options):

- `OutboundRequestsPerSecond`: refill rate.
- `OutboundBurstSize`: maximum immediate burst.
- `OutboundQueueLimit`: waiting request queue size.

Suggested starting point for read-heavy apps:

- `OutboundRequestsPerSecond = 10`
- `OutboundBurstSize = 20`
- `OutboundQueueLimit = 100`

If you see local throttling, increase burst and/or queue gradually. If upstream throttling (`429`) appears, lower request rate.

## Token storage

Default OAuth token storage is in-memory and process-local. For production apps, register a custom `ITokenProvider` before `AddYouVersionOAuth(...)`.

```csharp
builder.Services.AddSingleton<ITokenProvider, MyPersistentTokenProvider>();
```

## Troubleshooting

- `InvalidOperationException` mentioning `AddYouVersionApiClients`: call order is wrong; register API clients first.
- `YouVersionApiException` with `401`/`403`: check app key and (for write ops) OAuth token state.
- Local outbound throttling errors: tune `OutboundRequestsPerSecond`, `OutboundBurstSize`, and `OutboundQueueLimit`.

## Additional docs

- [Getting started](../docs/getting-started.md)
- [Authentication (app key)](../docs/authentication.md)
- [OAuth guide](../docs/oauth-guide.md)

## Build and pack

```powershell
dotnet pack -c Release
```
