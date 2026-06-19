# region usings
using Microsoft.AspNetCore.Components;
using Platform.API.Models;
#endregion
namespace Platform.SDK.Components.BibleComponents
{
    public partial class BookPicker
    {
        private IReadOnlyList<Book> _books = [];
        private int _loadedForVersion;
        private bool _loading;
        private string? _error;

        protected override void OnInitialized()
            => State.OnStateChanged += OnStateChangedHandler;

        protected override async Task OnParametersSetAsync()
            => await LoadBooksIfNeededAsync();

        private void OnStateChangedHandler()
            => InvokeAsync(async () =>
            {
                await LoadBooksIfNeededAsync();
                StateHasChanged();
            });

        private async Task LoadBooksIfNeededAsync()
        {
            var version = State.SelectedVersion;
            if (version is null || version.Id == _loadedForVersion) return;

            _loading = true;
            _error = null;
            _books = [];
            _loadedForVersion = version.Id;
            StateHasChanged();

            try
            {
                _books = await BookService.GetBooksAsync(version.Id);
            }
            catch (Exception ex)
            {
                _error = $"Could not load books: {ex.Message}";
            }
            finally
            {
                _loading = false;
            }
        }

        private void OnBookChanged(ChangeEventArgs e)
        {
            var usfm = e.Value?.ToString();
            var book = _books.FirstOrDefault(b => b.Usfm == usfm);
            if (book is not null)
                State.SelectBook(book);
        }

        public void Dispose()
            => State.OnStateChanged -= OnStateChangedHandler;
    }
}
