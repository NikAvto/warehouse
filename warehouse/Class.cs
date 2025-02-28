using SQLite;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Maui.Storage;

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

    public async Task DeleteAllAsync()
    {
        await _database.DeleteAllAsync<Item>();
        await _database.DeleteAllAsync<StorageLocation>();
    }

    // Метод для вставки нескольких записей в таблицу
    public async Task InsertAllAsync<T>(IEnumerable<T> items) where T : new()
    {
        foreach (var item in items)
        {
            await _database.InsertAsync(item);
        }
    }
    public async Task ExportDataAsync(string filePath)
    {
        // Получаем данные из базы
        var items = await _database.Table<Item>().ToListAsync();
        var locations = await _database.Table<StorageLocation>().ToListAsync();

        // Формируем объект для экспорта
        var exportData = new
        {
            Items = items,
            StorageLocations = locations
        };

        // Сериализуем в JSON
        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });

        // Сохраняем в файл
        await File.WriteAllTextAsync(filePath, json);
    }
    public class ImportData
    {
        public List<Item> Items { get; set; }
        public List<StorageLocation> StorageLocations { get; set; }
    }
    public async Task ImportDataAsync(string filePath)
    {
        try
        {
            // Читаем JSON из файла
            var json = await File.ReadAllTextAsync(filePath);

            // Десериализуем JSON
            var importData = JsonSerializer.Deserialize<ImportData>(json);

            if (importData != null)
            {
                // Очищаем старые данные
                await DeleteAllAsync();

                // Сохраняем новые данные
                await InsertAllAsync(importData.StorageLocations);
                await InsertAllAsync(importData.Items);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка при импорте данных: {ex.Message}");
        }
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

