using Platform.API.Models;

namespace Platform.SDK.Services
{
    public interface IPassageService
    {
        Task<Passage> GetPassageAsync(
            int versionId,
            string usfm,
            PassageRequestOptions? options = null,
            CancellationToken cancellationToken = default);
    }
}

