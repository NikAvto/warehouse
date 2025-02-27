using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;

namespace warehouse
{
    public partial class SearchPage : ContentPage
    {
        private readonly Database _database;
        public ObservableCollection<ItemViewModel> FilteredItems { get; set; } = new();
        private List<ItemViewModel> _allItems = new();

        public SearchPage(Database database)
        {
            InitializeComponent();
            _database = database;
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _allItems = await _database.GetAllItemsAsync();
            UpdateFilteredItems("");
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateFilteredItems(e.NewTextValue);
        }

        private void UpdateFilteredItems(string searchText)
        {
            FilteredItems.Clear();
            var filtered = _allItems
                .Where(item => string.IsNullOrWhiteSpace(searchText) ||
                               item.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var item in filtered)
            {
                FilteredItems.Add(item);
            }

            NoResultsLabel.IsVisible = FilteredItems.Count == 0;
        }
    }
}
