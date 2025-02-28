namespace warehouse;

public partial class DeletePage : ContentPage
{
    private readonly Database _database;
    private readonly Item _item;
    public DeletePage(Database database, Item item)
	{
		InitializeComponent();
		_database = database;
		_item = item;
	}
	private async void OnConfirmDelete(object sender, EventArgs e)
	{
        await _database.DeleteItemAsync(_item.Id);
        await Navigation.PopModalAsync();
    }
	private async void OnCancelDel(object sender, EventArgs e) 
	{
        await Navigation.PopModalAsync();
    }
}