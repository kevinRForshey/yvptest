using Microsoft.Extensions.Caching.Hybrid;
using Platform.API.Configuration;
using Platform.API.Models;

namespace Platform.API.Clients;

internal sealed class CachingBibleClient(
    BibleClient inner,
    HybridCache cache,
    YouVersionCacheOptions opts) : IBibleClient
{
    public async Task<PagedResult<BibleVersionSummary>> GetVersionsAsync(
        string languageRange = "en",
        string? pageToken = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var key = $"yv:versions:{languageRange}:{pageToken}:{pageSize}";
        return await cache.GetOrCreateAsync(
            key,
            ct => new ValueTask<PagedResult<BibleVersionSummary>>(
                inner.GetVersionsAsync(languageRange, pageToken, pageSize, ct)),
            new HybridCacheEntryOptions { Expiration = opts.VersionsTtl },
            cancellationToken: cancellationToken);
    }

    public async Task<BibleVersion> GetVersionAsync(int versionId, CancellationToken cancellationToken = default)
    {
        var key = $"yv:version:{versionId}";
        return await cache.GetOrCreateAsync(
            key,
            ct => new ValueTask<BibleVersion>(inner.GetVersionAsync(versionId, ct)),
            new HybridCacheEntryOptions { Expiration = opts.VersionsTtl },
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<Book>> GetBooksAsync(int versionId, CancellationToken cancellationToken = default)
    {
        var key = $"yv:books:{versionId}";
        return await cache.GetOrCreateAsync(
            key,
            ct => new ValueTask<IReadOnlyList<Book>>(inner.GetBooksAsync(versionId, ct)),
            new HybridCacheEntryOptions { Expiration = opts.BooksTtl },
            cancellationToken: cancellationToken);
    }

    public Task<IReadOnlyList<Book>> GetBooksAsync(BibleVersion version) => inner.GetBooksAsync(version);
}
