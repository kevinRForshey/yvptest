using Microsoft.AspNetCore.Components;

namespace PlatformTestApp.Components.Bible
{
    public partial class ChapterPicker
    {
        protected override void OnInitialized()
       => State.OnStateChanged += OnStateChangedHandler;

        private void OnStateChangedHandler()
            => InvokeAsync(StateHasChanged);

        private void OnChapterChanged(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out var ch))
                State.SelectChapter(ch);
        }

        public void Dispose()
            => State.OnStateChanged -= OnStateChangedHandler;
    }
}
