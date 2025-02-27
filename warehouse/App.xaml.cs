namespace warehouse
{
    public partial class App : Application
    {
        public App(Database database)
        {
            InitializeComponent();
            MainPage = new NavigationPage(new MainPage(database));
        }
    }
}
