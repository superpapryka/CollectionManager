using System.Diagnostics;
using CollectionManager.Models;
using Microsoft.Maui.Storage;

namespace CollectionManager.Services;

public class FileService
{
    public string RootFolder { get; }
    public string CollectionsFolder { get; }

    public FileService()
    {
        RootFolder = Path.Combine(FileSystem.AppDataDirectory, "CollectionManager");
        CollectionsFolder = Path.Combine(RootFolder, "Collections");

        Directory.CreateDirectory(RootFolder);
        Directory.CreateDirectory(CollectionsFolder);

        Debug.WriteLine($"App data path: {RootFolder}");
    }

    public async Task<List<CollectionDefinition>> GetCollectionsAsync()
    {
        var result = new List<CollectionDefinition>();

        foreach (var file in Directory.EnumerateFiles(CollectionsFolder, "*.txt"))
        {
            var loaded = await LoadCollectionFromFileAsync(file);

            if (loaded.Collection != null)
            {
                loaded.Collection.FilePath = file;
                loaded.Collection.ItemCount = loaded.Items.Count;
                result.Add(loaded.Collection);
            }
        }

        return result;
    }

    public async Task<(CollectionDefinition? Collection, List<CollectibleItem> Items)> LoadCollectionFromFileAsync(string filePath)
    {
        var items = new List<CollectibleItem>();

        if (!File.Exists(filePath))
            return (null, items);

        var lines = await File.ReadAllLinesAsync(filePath);
        if (lines.Length == 0)
            return (null, items);

        var header = lines[0].Split('|');

        var collection = new CollectionDefinition
        {
            Name = header[1],
            Description = header[2],
            Id = header[3],
            FilePath = filePath
        };

        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split('|');
            if (parts.Length < 7)
                continue;

            items.Add(new CollectibleItem
            {
                Id = parts[1],
                Name = parts[2],
                Category = parts[3],
                Quantity = int.Parse(parts[4]),
                Status = Enum.Parse<ItemStatus>(parts[5]),
                Notes = parts[6],
                ImagePath = parts.Length > 7 ? parts[7] : ""
            });
        }

        return (collection, items);
    }

    public async Task<CollectionDefinition> CreateCollectionAsync(string name, string description)
    {
        var id = Guid.NewGuid().ToString("N");
        var filePath = Path.Combine(CollectionsFolder, $"{name}_{id}.txt");

        var collection = new CollectionDefinition
        {
            Id = id,
            Name = name,
            Description = description,
            FilePath = filePath
        };

        await SaveCollectionAsync(collection, new List<CollectibleItem>());
        return collection;
    }

    public async Task SaveCollectionAsync(CollectionDefinition collection, IEnumerable<CollectibleItem> items)
    {
        var lines = new List<string>
        {
            $"COLLECTION|{collection.Name}|{collection.Description}|{collection.Id}"
        };

        foreach (var item in items)
        {
            lines.Add($"ITEM|{item.Id}|{item.Name}|{item.Category}|{item.Quantity}|{item.Status}|{item.Notes}|{item.ImagePath}");
        }

        await File.WriteAllLinesAsync(collection.FilePath, lines);
    }

    public async Task DeleteCollectionAsync(CollectionDefinition collection)
    {
        if (File.Exists(collection.FilePath))
            File.Delete(collection.FilePath);

        await Task.CompletedTask;
    }

    public async Task<string> ExportCollectionAsync(CollectionDefinition collection)
    {
        var exportFolder = Path.Combine(RootFolder, "Exports");
        Directory.CreateDirectory(exportFolder);

        var exportPath = Path.Combine(exportFolder, $"{collection.Name}.txt");

        File.Copy(collection.FilePath, exportPath, true);

        return await Task.FromResult(exportPath);
    }

    public async Task ImportIntoCollectionAsync(CollectionDefinition targetCollection, string sourceFilePath)
    {
        var imported = await LoadCollectionFromFileAsync(sourceFilePath);
        var current = await LoadCollectionFromFileAsync(targetCollection.FilePath);

        var items = current.Items;

        foreach (var item in imported.Items)
        {
            if (!items.Any(x => x.Id == item.Id))
                items.Add(item);
        }

        await SaveCollectionAsync(targetCollection, items);
    }
}