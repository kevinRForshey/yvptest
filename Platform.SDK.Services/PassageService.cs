#region usings
using Platform.API.Clients;
using Platform.API.Models;
using YouVersion.UsfmReferences;
#endregion

namespace Platform.SDK.Services
{
    /// <summary>
    /// Service wrapper around <see cref="IPassageClient"/> for convenient passage retrieval.
    /// Accepts typed USFM references for type-safe scripture access.
    /// </summary>
    public sealed class PassageService(IPassageClient client)
    {
        /// <summary>
        /// Retrieves a Bible passage using a typed USFM reference.
        /// </summary>
        /// <param name="versionId">The numeric Bible version id.</param>
        /// <param name="reference">The USFM passage reference.</param>
        /// <param name="options">Optional passage request options (format, headings, notes).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>The requested passage.</returns>
        public Task<Passage> GetPassageAsync(
            int versionId,
            Reference reference,
            PassageRequestOptions? options = null,
            CancellationToken cancellationToken = default)
            => client.GetPassageAsync(versionId, reference, options, cancellationToken);
    }
}
