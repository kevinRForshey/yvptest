#region  usings
using Microsoft.AspNetCore.Components;
using Platform.API.Models;
using Platform.API.OAuth;
using Platform.SDK.Services;
#endregion

namespace Platform.SDK.Components.BibleComponents
{
    public partial class BibleReader
    {
        [Inject]
        private IBibleReaderStateService State { get; set; } = default!;

        [Inject]
        private ITokenProvider TokenProvider { get; set; } = default!;

        [Inject]
        private NavigationManager Nav { get; set; } = default!;

        [Inject]
        private PassageService PassageService { get; set; } = default!;

        // Populated when the OAuth callback redirects with ?oauth_error=...
        [SupplyParameterFromQuery(Name = "oauth_error")]
        public string? OAuthError { get; set; }

        [SupplyParameterFromQuery(Name = "auth_mode")]
        public string? AuthMode { get; set; }

        private Passage? _passage;
        private bool _loading;
        private string? _error;
        private CancellationTokenSource? _cts;

        private bool _isSignedIn;
        private string? _userName;
        private bool _debugTokenPresent;
        private string? _debugIdentity;
        private string? _copyright;

        protected override async Task OnInitializedAsync()
        {
            State.OnStateChanged += OnStateChangedHandler;
            await CheckSignInAsync();
        }

        private async Task CheckSignInAsync()
        {
            var token = await TokenProvider.GetTokenAsync();
            var signedIn = token is not null && !token.IsExpired();
            var userName = token?.GetDisplayIdentity();
            var tokenPresent = token is not null;

            if (signedIn != _isSignedIn || userName != _userName || tokenPresent != _debugTokenPresent)
            {
                _isSignedIn = signedIn;
                _userName = userName;
                _debugTokenPresent = tokenPresent;
                _debugIdentity = userName;

                await InvokeAsync(StateHasChanged);
            }
        }

        private bool CanRead =>
            State.SelectedVersion is not null &&
            State.SelectedBook is not null &&
            State.SelectedChapter is not null &&
            State.SelectedVerseStart is not null;

        private async Task ReadPassageAsync()
        {
            if (!CanRead)
                return;

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            _loading = true;
            _error = null;
            _passage = null;

            try
            {
                var usfm = BuildUsfm();

                _passage = await PassageService.GetPassageAsync(
                    State.SelectedVersion!.Id,
                    usfm,
                    new PassageRequestOptions { Format = PassageFormat.Html },
                    _cts.Token);

                _copyright = State.SelectedVersion.Copyright;
            }
            catch (OperationCanceledException)
            {
                // Superseded by a newer request — ignore
            }
            catch (Exception ex)
            {
                _error = $"Could not load passage: {ex.Message}";
            }
            finally
            {
                _loading = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        private string BuildUsfm()
        {
            var book = State.SelectedBook!.Usfm;
            var chapter = State.SelectedChapter!.Value;
            var verseStart = State.SelectedVerseStart!.Value;
            var verseEnd = State.SelectedVerseEnd;

            return verseEnd.HasValue && verseEnd.Value != verseStart
                ? $"{book}.{chapter}.{verseStart}-{verseEnd.Value}"
                : $"{book}.{chapter}.{verseStart}";
        }

        private void SignIn() => Nav.NavigateTo("/auth/login", forceLoad: true);
        private void SignOut() => Nav.NavigateTo("/auth/logout", forceLoad: true);
        private void ClearAuthDebugState() => Nav.NavigateTo("/auth/logout", forceLoad: true);

        private void OnStateChangedHandler()
            => InvokeAsync(() =>
            {
                _passage = null;
                _error = null;
                StateHasChanged();
            });

        public void Dispose()
        {
            State.OnStateChanged -= OnStateChangedHandler;
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}