using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Platform.API.Clients;
using Platform.API.Extensions;
using Platform.API.OAuth;
using Xunit;

namespace Platform.API.Tests.Extensions;

public sealed class ServiceCollectionExtensionsTests
{
    // -------------------------------------------------------------------------
    // AddYouVersionApiClients — inline options
    // -------------------------------------------------------------------------

    [Fact]
    public void AddYouVersionApiClients_RegistersIBibleClient()
    {
        var sp = BuildProvider(withOAuth: false);
        sp.GetRequiredService<IBibleClient>().Should().NotBeNull();
    }

    [Fact]
    public void AddYouVersionApiClients_RegistersIPassageClient()
    {
        var sp = BuildProvider(withOAuth: false);
        sp.GetRequiredService<IPassageClient>().Should().NotBeNull();
    }

    [Fact]
    public void AddYouVersionApiClients_RegistersIHighlightClient()
    {
        var sp = BuildProvider(withOAuth: false);
        sp.GetRequiredService<IHighlightClient>().Should().NotBeNull();
    }

    [Fact]
    public void AddYouVersionApiClients_ResolvesDistinctInstances_PerScope()
    {
        var sp = BuildProvider(withOAuth: false);

        using var scope1 = sp.CreateScope();
        using var scope2 = sp.CreateScope();

        var client1 = scope1.ServiceProvider.GetRequiredService<IBibleClient>();
        var client2 = scope2.ServiceProvider.GetRequiredService<IBibleClient>();

        // Typed HttpClient registrations create a new instance per resolution
        client1.Should().NotBeSameAs(client2);
    }

    // -------------------------------------------------------------------------
    // AddYouVersionOAuth
    // -------------------------------------------------------------------------

    [Fact]
    public void AddYouVersionOAuth_RegistersIYouVersionOAuthClient()
    {
        var sp = BuildProvider(withOAuth: true);
        sp.GetRequiredService<IYouVersionOAuthClient>().Should().NotBeNull();
    }

    [Fact]
    public void AddYouVersionOAuth_RegistersITokenProvider_AsInMemoryTokenProvider()
    {
        var sp = BuildProvider(withOAuth: true);
        sp.GetRequiredService<ITokenProvider>().Should().BeOfType<InMemoryTokenProvider>();
    }

    [Fact]
    public void AddYouVersionOAuth_ReturnsServiceCollection_ForChaining()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var returned = services.AddYouVersionApiClients(o => o.AppKey = "key")
                               .AddYouVersionOAuth(o => o.ClientId = "cid");

        returned.Should().BeSameAs(services);
    }

    [Fact]
    public void AddYouVersionOAuth_ThrowsInvalidOperationException_WhenApiClientsNotRegisteredFirst()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var act = () => services.AddYouVersionOAuth(o => o.ClientId = "cid");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*AddYouVersionApiClients*");
    }

    [Fact]
    public void AddYouVersionApiClients_ThrowsArgumentNullException_WhenServicesIsNull()
    {
        IServiceCollection services = null!;

        var act = () => services.AddYouVersionApiClients(o => o.AppKey = "key");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddYouVersionApiClients_ThrowsArgumentNullException_WhenConfigureIsNull()
    {
        var services = new ServiceCollection();

        var act = () => services.AddYouVersionApiClients((Action<Configuration.YouVersionApiOptions>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // -------------------------------------------------------------------------
    // Custom ITokenProvider replacement
    // -------------------------------------------------------------------------

    [Fact]
    public void AddYouVersionOAuth_AllowsCustomTokenProvider_Override()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddYouVersionApiClients(o => o.AppKey = "key");

        // Register custom provider BEFORE AddYouVersionOAuth
        services.AddSingleton<ITokenProvider, CustomTokenProvider>();
        services.AddYouVersionOAuth(o => o.ClientId = "cid");

        // The first registered singleton wins
        var sp = services.BuildServiceProvider();
        sp.GetRequiredService<ITokenProvider>().Should().BeOfType<CustomTokenProvider>();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static ServiceProvider BuildProvider(bool withOAuth)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddYouVersionApiClients(o => o.AppKey = "test-key");

        if (withOAuth)
        {
            services.AddYouVersionOAuth(o =>
            {
                o.ClientId = "test-client";
                o.RedirectUri = new Uri("https://localhost/callback");
            });
        }

        return services.BuildServiceProvider();
    }

    private sealed class CustomTokenProvider : ITokenProvider
    {
        public System.Threading.Tasks.Task<OAuthTokenResponse?> GetTokenAsync(
            System.Threading.CancellationToken ct = default)
            => System.Threading.Tasks.Task.FromResult<OAuthTokenResponse?>(null);

        public System.Threading.Tasks.Task StoreTokenAsync(OAuthTokenResponse token,
            System.Threading.CancellationToken ct = default)
            => System.Threading.Tasks.Task.CompletedTask;

        public System.Threading.Tasks.Task ClearTokenAsync(
            System.Threading.CancellationToken ct = default)
            => System.Threading.Tasks.Task.CompletedTask;
    }
}
