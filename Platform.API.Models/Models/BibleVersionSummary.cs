using System.Text.Json.Serialization;

namespace Platform.API.Models;

/// <summary>
/// A lightweight summary of a Bible version returned by the list-versions endpoint.
/// </summary>
public sealed record BibleVersionSummary
{
    /// <summary>Bible Version ID</summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>Short abbreviation for the version (e.g. <c>NIV</c>, <c>BSB</c>).</summary>
    [JsonPropertyName("abbreviation")]
    public string Abbreviation { get; init; } = string.Empty;

    /// <summary>Abbreviation localized to the version's language.</summary>
    [JsonPropertyName("localized_abbreviation")]
    public string LocalizedAbbreviation { get; init; } = string.Empty;

    /// <summary>Full title of the Bible version.</summary>
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    /// <summary>Title localized to the version's language.</summary>
    [JsonPropertyName("localized_title")]
    public string LocalizedTitle { get; init; } = string.Empty;

    /// <summary>BCP-47 language tag (e.g. <c>en</c>, <c>es</c>).</summary>
    [JsonPropertyName("language_tag")]
    public string LanguageTag { get; init; } = string.Empty;
}
