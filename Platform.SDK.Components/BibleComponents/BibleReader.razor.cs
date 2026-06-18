using Microsoft.AspNetCore.Components;

using Platform.API.OAuth;
using Platform.SDK.Services;

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
        // Populated when the OAuth callback redirects with ?oauth_error=...
        [SupplyParameterFromQuery(Name = "oauth_error")]
        public string? OAuthError { get; set; }

        [SupplyParameterFromQuery(Name = "auth_mode")]
        public string? AuthMode { get; set; }

        private PassageDisplay? _passageDisplay;
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

        // Re-check sign-in state on the first interactive render so the circuit always
        // reflects the token stored during the OAuth callback, even if the prerender and
        // circuit DI scopes briefly disagree.
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
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
                StateHasChanged();
            }
        }

        private bool CanRead =>
            State.SelectedVersion is not null &&
            State.SelectedBook is not null &&
            State.SelectedChapter is not null &&
            State.SelectedVerseStart is not null;

        private async Task ReadPassageAsync()
        {
            if (_passageDisplay is not null)
                await _passageDisplay.LoadAsync();
        }

        private void SignIn() => Nav.NavigateTo("/auth/login", forceLoad: true);
        private void SignOut() => Nav.NavigateTo("/auth/logout", forceLoad: true);
        private void ClearAuthDebugState() => Nav.NavigateTo("/auth/logout", forceLoad: true);

        private void OnStateChangedHandler()
            => InvokeAsync(StateHasChanged);

        public void Dispose()
            => State.OnStateChanged -= OnStateChangedHandler;
    }
}
