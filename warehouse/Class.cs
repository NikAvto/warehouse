using SQLite;
using System.Collections.Generic;
namespace warehouse;
public class Database
{
    private readonly SQLiteAsyncConnection _database;

    public Database(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
        _database.CreateTableAsync<StorageLocation>().Wait();
        _database.CreateTableAsync<Item>().Wait();
    }

    public Task<List<StorageLocation>> GetStorageLocationsAsync() =>
        _database.Table<StorageLocation>().ToListAsync();

    public Task<int> SaveStorageLocationAsync(StorageLocation location) =>
        _database.InsertAsync(location);

    public Task<int> DeleteStorageLocationAsync(StorageLocation location) =>
        _database.DeleteAsync(location);

    public Task<List<Item>> GetItemsAsync(int storageLocationId) =>
        _database.Table<Item>().Where(i => i.StorageLocationId == storageLocationId).ToListAsync();

    public Task<int> SaveItemAsync(Item item) =>
        _database.InsertAsync(item);

    public Task<int> DeleteItemAsync(Item item) =>
        _database.DeleteAsync(item);
    public async Task<List<ItemViewModel>> GetAllItemsAsync()
    {
        var items = await _database.Table<Item>().ToListAsync();
        var locations = await _database.Table<StorageLocation>().ToListAsync();

        var itemViewModels = items.Select(item =>
            new ItemViewModel(item,
                locations.FirstOrDefault(l => l.Id == item.StorageLocationId)?.Name ?? "Неизвестное место"))
            .ToList();

        return itemViewModels;
    }
    public async Task UpdateItemStorageAsync(int itemId, int newStorageId)
    {
        var item = await _database.Table<Item>().FirstOrDefaultAsync(i => i.Id == itemId);
        if (item != null)
        {
            item.StorageLocationId = newStorageId;
            await _database.UpdateAsync(item);
        }
    }
    public async Task DeleteItemAsync(int itemId)
    {
        var item = await _database.Table<Item>().FirstOrDefaultAsync(i => i.Id == itemId);
        if (item != null)
        {
            await _database.DeleteAsync(item);
        }
    }

    public async Task AddItemAsync(Item item)
    {
        await _database.InsertAsync(item);
    }


}

public class StorageLocation
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
}

public class Item
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
    public int StorageLocationId { get; set; }
}
public class ItemViewModel
{
    public string Name { get; set; }
    public string StorageLocation { get; set; }

    public string DisplayText => $"{Name} (Место: {StorageLocation})";

    public ItemViewModel(Item item, string storageLocation)
    {
        Name = item.Name;
        StorageLocation = storageLocation;
    }
}

