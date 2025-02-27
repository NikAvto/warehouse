using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
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
    }
}