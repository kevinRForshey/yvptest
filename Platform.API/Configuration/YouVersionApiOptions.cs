using System;
using System.ComponentModel.DataAnnotations;

namespace Platform.API.Configuration;

/// <summary>
/// Configuration options for the YouVersion Platform API client.
/// Bind to the <c>YouVersionApi</c> configuration section or supply values directly.
/// </summary>
public sealed class YouVersionApiOptions
{
    /// <summary>
    /// The configuration section name to bind from <c>appsettings.json</c> or environment variables.
    /// </summary>
    public const string SectionName = "YouVersionApi";

    /// <summary>
    /// Your YouVersion Platform app key. Sent as the <c>X-YVP-App-Key</c> header on every request.
    /// App keys are <em>not</em> secrets and may be included in source code or client bundles.
    /// Obtain one at <see href="https://platform.youversion.com"/>.
    /// </summary>
    [Required]
    public string AppKey { get; set; } = string.Empty;

    /// <summary>
    /// The base URL of the YouVersion Platform API.
    /// Defaults to <c>https://api.youversion.com</c>.
    /// Override only in testing scenarios.
    /// </summary>
    public Uri BaseAddress { get; set; } = new("https://api.youversion.com");

    /// <summary>
    /// HTTP request timeout. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Number of outbound API requests replenished per second for the SDK's per-client
    /// token-bucket limiter.
    /// </summary>
    public int OutboundRequestsPerSecond { get; set; } = 10;

    /// <summary>
    /// Maximum burst size for outbound requests before queuing/rate-limit rejection.
    /// Must be greater than or equal to <see cref="OutboundRequestsPerSecond"/>.
    /// </summary>
    public int OutboundBurstSize { get; set; } = 20;

    /// <summary>
    /// Maximum number of queued outbound requests waiting for rate-limit permits.
    /// </summary>
    public int OutboundQueueLimit { get; set; } = 100;
}
