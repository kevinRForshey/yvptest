using Platform.API.Clients;
using Platform.API.Models;

namespace Platform.SDK.Services
{
    public sealed class PassageService(IPassageClient client) : IPassageService
    {
        public Task<Passage> GetPassageAsync(
            int versionId,
            string usfm,
            PassageRequestOptions? options = null,
            CancellationToken cancellationToken = default)
            => client.GetPassageAsync(versionId, usfm, options, cancellationToken);
    }
}
