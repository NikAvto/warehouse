using System.Collections.ObjectModel;

namespace warehouse;

public partial class MoveItemPage : ContentPage
{
    private readonly Database _database;
    private readonly Item _item;
    public ObservableCollection<StorageLocation> StorageLocations { get; set; }
    public StorageLocation SelectedLocation { get; set; }
    public string ItemName => _item.Name;

    public MoveItemPage(Database database, Item item, List<StorageLocation> locations)
    {
        InitializeComponent();
        _database = database;
        _item = item;
        StorageLocations = new ObservableCollection<StorageLocation>(locations);
        BindingContext = this;
    }

    private async void OnConfirmMove(object sender, EventArgs e)
    {
        if (SelectedLocation == null)
        {
            await DisplayAlert("Ошибка", "Выберите место хранения", "OK");
            return;
        }

        _item.StorageLocationId = SelectedLocation.Id;
        await _database.SaveItemAsync(_item);

        await Navigation.PopModalAsync(); // Закрываем окно
    }

    private async void OnCancel(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync(); // Закрываем окно без изменений
    }
}
