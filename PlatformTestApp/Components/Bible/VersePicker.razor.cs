using Microsoft.AspNetCore.Components;

namespace PlatformTestApp.Components.Bible
{
    public partial class VersePicker
    {
        private const int MaxVerse = 176; // Psalm 119 — longest chapter

        private int _verseStart = 1;
        private int? _verseEnd;
        private string? _validationError;
        private int? _lastChapter;

        protected override void OnInitialized()
            => State.OnStateChanged += OnStateChangedHandler;

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
