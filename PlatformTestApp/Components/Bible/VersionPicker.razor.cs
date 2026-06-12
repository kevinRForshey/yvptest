using Microsoft.AspNetCore.Components;

using Platform.API.Models;

namespace PlatformTestApp.Components.Bible
{
    public partial class VersionPicker
    {
        private IReadOnlyList<BibleVersionSummary> _versions = [];
        private BibleVersionSummary? _selected;
        private bool _loading = true;
        private string? _error;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                _versions = await VersionService.GetVersionsAsync();
            }
            catch (Exception ex)
            {
                _error = $"Could not load Bible versions: {ex.Message}";
            }
            finally
            {
                _loading = false;
            }
        }

        private void OnVersionChanged(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out var id))
            {
                _selected = _versions.FirstOrDefault(v => v.Id == id);
                if (_selected is not null)
                    State.SelectVersion(_selected);
            }
            else
            {
                _selected = null;
                State.Reset();
            }
        }
    }
}
