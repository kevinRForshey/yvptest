#region usings
using Microsoft.AspNetCore.Components;
using Platform.API.Models;
using YouVersion.UsfmReferences;

#endregion
namespace Platform.SDK.Components.BibleComponents
{
    public partial class PassageDisplay
    {
        [Parameter, EditorRequired]public Passage Passage { get; set; } = default!;
        
        [Parameter]
        public string? Copyright { get; set; }

        private Passage? _passage;
        private bool _loading;
        private string? _error;
        private string? _copyright;
        private CancellationTokenSource? _cts;

        protected override void OnInitialized()
            => State.OnStateChanged += OnStateChangedHandler;

        private void OnStateChangedHandler()
            => InvokeAsync(() =>
            {
                _passage = null;
                _error = null;
                StateHasChanged();
            });

        public async Task LoadAsync()
        {
            if (State.SelectedVersion is null || State.SelectedBook is null ||
                State.SelectedChapter is null || State.SelectedVerseStart is null)
                return;

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            _loading = true;
            _error = null;
            _passage = null;
            StateHasChanged();

            try
            {
                var reference = BuildReference();
                _passage = await PassageService.GetPassageAsync(
                    State.SelectedVersion.Id,
                    reference,
                    new PassageRequestOptions { Format = PassageFormat.Html },
                    _cts.Token);
                _copyright = Copyright;
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

        private YouVersion.UsfmReferences.Reference BuildReference()
        {
            var book = State.SelectedBook!.Usfm;
            var chapter = State.SelectedChapter!.Value;
            var verseStart = State.SelectedVerseStart!.Value;
            var verseEnd = State.SelectedVerseEnd ?? verseStart;

            return new YouVersion.UsfmReferences.Reference(
                book: book,
                chapter: chapter,
                verses:
                [
                    new YouVersion.UsfmReferences.VerseRange(verseStart, verseEnd)
                ]);
        }

        public void Dispose()
        {
            State.OnStateChanged -= OnStateChangedHandler;
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
