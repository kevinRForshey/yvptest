using Microsoft.AspNetCore.Components;

namespace Platform.SDK.Components.BibleComponents
{
    public partial class VersePicker
    {
        [Parameter, EditorRequired] public string Book { get; set; } = string.Empty;
        [Parameter, EditorRequired] public int Chapter { get; set; }
        [Parameter] public int Verse { get; set; }       // maps from SelectedVerseStart
        [Parameter] public int VerseEnd { get; set; }    // maps from SelectedVerseEnd
        [Parameter, EditorRequired] public int VersionId { get; set; }
        private const int MaxVerse = 176; // Psalm 119 — longest chapter

        private int _verseStart = 1;
        private int? _verseEnd;
        private string? _validationError;
        private int? _lastChapter;

        protected override void OnInitialized()
        {
            // Seed _lastChapter from the current state before subscribing.
            // VersePicker only mounts after a chapter is already selected, so
            // _lastChapter would otherwise stay null and the first notification
            // (even one the user triggers via verse input) would falsely look like
            // a chapter change and reset _verseStart back to 1.
            _lastChapter = State.SelectedChapter;
            State.OnStateChanged += OnStateChangedHandler;
        }

        private void OnStateChangedHandler()
            => InvokeAsync(() =>
            {
                if (State.SelectedChapter != _lastChapter)
                {
                    _lastChapter = State.SelectedChapter;
                    _verseStart = 1;
                    _verseEnd = null;
                    _validationError = null;
                    if (State.SelectedChapter is not null)
                        State.SelectVerseRange(1, null);
                }
                StateHasChanged();
            });

        private void OnStartChanged(ChangeEventArgs e)
        {
            _validationError = null;
            if (int.TryParse(e.Value?.ToString(), out var v) && v >= 1)
            {
                _verseStart = v;
                if (_verseEnd.HasValue && _verseEnd < _verseStart)
                    _verseEnd = null;
                Commit();
            }
            else
            {
                _validationError = "Please enter a valid verse number.";
            }
        }

        private void OnEndChanged(ChangeEventArgs e)
        {
            _validationError = null;
            var raw = e.Value?.ToString();
            if (string.IsNullOrWhiteSpace(raw))
            {
                _verseEnd = null;
                Commit();
                return;
            }

            if (int.TryParse(raw, out var v) && v >= _verseStart)
            {
                _verseEnd = v;
                Commit();
            }
            else
            {
                _validationError = "End verse must be ≥ start verse.";
            }
        }

        private void ClearRange()
        {
            _verseStart = 1;
            _verseEnd = null;
            _validationError = null;
            Commit();
        }

        private void Commit() => State.SelectVerseRange(_verseStart, _verseEnd);

        public void Dispose()
            => State.OnStateChanged -= OnStateChangedHandler;
    }
}
