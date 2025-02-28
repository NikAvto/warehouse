using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using static warehouse.Database;
namespace warehouse
{
    
    public partial class MainPage : ContentPage
    {
        private readonly Database _database;

        public ObservableCollection<StorageLocation> StorageLocations { get; set; } = new();

        public MainPage(Database database)
        {
            InitializeComponent();
            _database = database;
            BindingContext = this;
        }
        private bool isSwipeAction = false;

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            //ShowTutorial();
            var locations = await _database.GetStorageLocationsAsync();
            StorageLocations.Clear();
            foreach (var location in locations)
            {
                StorageLocations.Add(location);
            }
        }
        /*private async void ShowTutorial()
        {
            if (!Preferences.Get("HasSeenTutorial", false))
            {
                await Task.Delay(500); // Даем странице загрузиться

                // Подсказка над кнопкой "Поиск"
                this.ShowPopup(new TutorialPopup("Нажмите здесь, чтобы искать вещи."));

                await Task.Delay(3000); // Ждем перед следующей подсказкой

                // Подсказка над свайпом удаления
                this.ShowPopup(new TutorialPopup("Свайп влево для удаления места хранения."));

                await Task.Delay(3000);

                // Подсказка над кнопкой "Добавить место хранения"
                this.ShowPopup(new TutorialPopup("Нажмите здесь, чтобы добавить новое место хранения."));

                Preferences.Set("HasSeenTutorial", true); // Запоминаем, что подсказки показаны
            }
        }*/

        private async void OnAddStorageClicked(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync("Новое место", "Введите название:");
            if (!string.IsNullOrWhiteSpace(result))
            {
                var newLocation = new StorageLocation { Name = result };
                await _database.SaveStorageLocationAsync(newLocation);
                StorageLocations.Add(newLocation);
            }
        }

        private async void OnStorageSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is StorageLocation selectedLocation)
            {
                // Проверяем, не произошло ли нажатие во время свайпа
                if (isSwipeAction)
                {
                    isSwipeAction = false; // Сбрасываем флаг
                    return;
                }

                await Navigation.PushAsync(new ItemsPage(_database, selectedLocation));
            }
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SearchPage(_database));
        }
        private async void OnDeleteStorage(object sender, EventArgs e)
        {
            isSwipeAction = true;
            if (sender is SwipeItem swipeItem && swipeItem.BindingContext is StorageLocation location)
            {
                //isSwipeAction = true; // Флаг, что свайп выполняется

                bool confirm = await DisplayAlert("Удаление", $"Удалить место \"{location.Name}\"?", "Да", "Отмена");
                if (confirm)
                {
                    await _database.DeleteStorageLocationAsync(location);
                    StorageLocations.Remove(location);
                }
            }
            isSwipeAction = false;
        }
        public async Task ExportDataWithPickerAsync()
        {
            try
            {
                // Генерируем JSON-данные
                var items = await _database.GetAllItemsAsync();
                var locations = await _database.GetStorageLocationsAsync();

                var exportData = new
                {
                    Items = items,
                    StorageLocations = locations
                };

                var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });

                // Сохраняем файл
                var fileSaverResult = await FileSaver.Default.SaveAsync("warehouse_export.json", new MemoryStream(Encoding.UTF8.GetBytes(json)));

                if (fileSaverResult.IsSuccessful)
                {
                    await DisplayAlert("Успех", $"Файл успешно сохранен: {fileSaverResult.FilePath}", "OK");
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось сохранить файл", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось экспортировать данные: {ex.Message}", "OK");
            }
        }

        public async Task ImportDataWithPickerAsync()
        {
            try
            {
                // Выбираем файл для импорта
                var fileResult = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Выберите файл для импорта",
                    FileTypes = new FilePickerFileType(
                        new Dictionary<DevicePlatform, IEnumerable<string>>
                        {
                    { DevicePlatform.WinUI, new[] { ".json" } },
                    { DevicePlatform.Android, new[] { "application/json" } },
                    { DevicePlatform.iOS, new[] { "public.json" } }
                        })
                });

                if (fileResult != null)
                {
                    // Импортируем данные из выбранного файла
                    await _database.ImportDataAsync(fileResult.FullPath);
                    await DisplayAlert("Успех", "Данные успешно импортированы", "OK");

                    // Обновляем список мест хранения
                    var locations = await _database.GetStorageLocationsAsync();
                    StorageLocations.Clear();
                    foreach (var location in locations)
                    {
                        StorageLocations.Add(location);
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось импортировать данные: {ex.Message}", "OK");
            }
        }
        private async void OnExportDataClicked(object sender, EventArgs e)
        {
            await ExportDataWithPickerAsync();
        }

        private async void OnImportDataClicked(object sender, EventArgs e)
        {
            await ImportDataWithPickerAsync();
        }
    }
}