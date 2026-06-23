using System.Text.Json.Serialization;

namespace Platform.API.Models.Models;

/// <summary>
/// Represents a single verse within a Bible chapter.
/// </summary>
public sealed record Verse
{
    /// <summary>Gets the USFM verse identifier (e.g. <c>JHN.3.16</c>).</summary>
    /// <remarks>
    /// This is a normalized, validated USFM verse reference from the YouVersion Platform API.
    /// All USFM references passed to passage and highlight operations are validated against
    /// YouVersion.UsfmReferences.BookCatalog before being sent to the API.
    /// </remarks>
    [JsonPropertyName("usfm")]
    public string Usfm { get; init; } = string.Empty;

    /// <summary>Gets the human-readable verse reference (e.g. <c>John 3:16</c>).</summary>
    /// <value>The human-readable verse reference.</value>
    [JsonPropertyName("human")]
    public string Human { get; init; } = string.Empty;
    
    /// <summary>
    /// Holds the actual text of the verse. This is the content that will be displayed to users when they read the verse. It may contain formatting or special characters depending on the source of the verse text.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;
}
