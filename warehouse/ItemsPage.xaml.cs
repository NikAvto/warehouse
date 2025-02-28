using System.Collections.ObjectModel;
namespace warehouse
{
    public partial class ItemsPage : ContentPage
    {
        private readonly Database _database;
        private readonly StorageLocation _storageLocation;

        public ObservableCollection<Item> Items { get; set; } = new();
        public string StorageName => _storageLocation.Name;

        public ItemsPage(Database database, StorageLocation storageLocation)
        {
            InitializeComponent();
            _database = database;
            _storageLocation = storageLocation;
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var items = await _database.GetItemsAsync(_storageLocation.Id);
            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }
        }

        private async void OnAddItemClicked(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync("Новый предмет", "Введите название:");
            if (!string.IsNullOrWhiteSpace(result))
            {
                var newItem = new Item { Name = result, StorageLocationId = _storageLocation.Id };
                await _database.SaveItemAsync(newItem);
                Items.Add(newItem);
            }
        }
        private async void OnDeleteItem(object sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.BindingContext is Item item)
            {
                await Navigation.PushModalAsync(new DeletePage(_database, item));
            }
        }

        private async void OnMoveItem(object sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.BindingContext is Item item)
            {
                var locations = await _database.GetStorageLocationsAsync();
                var availableLocations = locations.Where(l => l.Id != _storageLocation.Id).ToList();

                if (availableLocations.Count == 0)
                {
                    await DisplayAlert("Ошибка", "Нет других мест хранения для переноса", "OK");
                    return;
                }

                await Navigation.PushModalAsync(new MoveItemPage(_database, item, availableLocations));
            }
        }

    }
}
