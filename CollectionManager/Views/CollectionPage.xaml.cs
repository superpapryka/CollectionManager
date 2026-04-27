using CollectionManager.Models;
using System.Collections.ObjectModel;

namespace CollectionManager.Views;

public partial class CollectionPage : ContentPage
{
    private readonly CollectionDefinition _collection;
    private readonly ObservableCollection<CollectibleItem> _items = new();

    public CollectionPage(CollectionDefinition collection)
    {
        InitializeComponent();

        _collection = collection;
        Title = collection.Name;

        CollectionNameLabel.Text = collection.Name;
        DescriptionLabel.Text = collection.Description;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var loaded = await App.Store.LoadCollectionFromFileAsync(_collection.FilePath);

        _items.Clear();
        foreach (var item in loaded.Items)
            _items.Add(item);

        ItemsView.ItemsSource = _items;
        CountLabel.Text = $"Items: {_items.Count}";
    }

    private async void AddItem_Clicked(object sender, EventArgs e)
    {
        await OpenEditorAsync(null);
    }

    private async void EditItem_Clicked(object sender, EventArgs e)
    {
        var selected = ItemsView.SelectedItem as CollectibleItem;
        if (selected is null)
        {
            await DisplayAlert("Info", "Select an item first.", "OK");
            return;
        }

        ItemsView.SelectedItem = null;
        await OpenEditorAsync(Clone(selected));
    }

    private async void DeleteItem_Clicked(object sender, EventArgs e)
    {
        var selected = ItemsView.SelectedItem as CollectibleItem;
        if (selected is null)
        {
            await DisplayAlert("Info", "Select an item first.", "OK");
            return;
        }

        var confirm = await DisplayAlert("Delete", $"Delete '{selected.Name}'?", "Yes", "No");
        if (!confirm)
            return;

        _items.Remove(selected);
        await App.Store.SaveCollectionAsync(_collection, _items);
        await LoadAsync();
    }

    private async void Summary_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SummaryPage(_collection, _items.ToList()));
    }

    private async void Export_Clicked(object sender, EventArgs e)
    {
        var path = await App.Store.ExportCollectionAsync(_collection);
        await DisplayAlert("Export complete", $"File saved to:\n{path}", "OK");
    }

    private async void Import_Clicked(object sender, EventArgs e)
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select a collection text file"
        });

        if (result is null)
            return;

        await App.Store.ImportIntoCollectionAsync(_collection, result.FullPath);
        await LoadAsync();
    }

    private async void DeleteCollection_Clicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "Delete collection",
            $"Do you want to delete collection '{_collection.Name}'?",
            "Yes",
            "No");

        if (!confirm)
            return;

        await App.Store.DeleteCollectionAsync(_collection);
        await Navigation.PopAsync();
    }

    private async Task OpenEditorAsync(CollectibleItem? existingItem)
    {
        var page = new EditItemPage(_collection, existingItem, async editedItem =>
        {
            var duplicate = _items.Any(x =>
                x.Id != editedItem.Id &&
                x.Name.Equals(editedItem.Name, StringComparison.OrdinalIgnoreCase));

            if (duplicate)
            {
                var confirm = await DisplayAlert(
                    "Duplicate item",
                    "An item with the same name already exists. Save anyway?",
                    "Save",
                    "Cancel");

                if (!confirm)
                    return;
            }

            var current = _items.FirstOrDefault(x => x.Id == editedItem.Id);

            if (current is null)
            {
                _items.Add(editedItem);
            }
            else
            {
                current.Name = editedItem.Name;
                current.Category = editedItem.Category;
                current.Quantity = editedItem.Quantity;
                current.Status = editedItem.Status;
                current.Notes = editedItem.Notes;
                current.ImagePath = editedItem.ImagePath;
            }

            await App.Store.SaveCollectionAsync(_collection, _items);
            await LoadAsync();
        });

        await Navigation.PushAsync(page);
    }

    private async void ItemsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        await Task.CompletedTask;
    }

    private static CollectibleItem Clone(CollectibleItem source)
    {
        return new CollectibleItem
        {
            Id = source.Id,
            Name = source.Name,
            Category = source.Category,
            Quantity = source.Quantity,
            Status = source.Status,
            Notes = source.Notes,
            ImagePath = source.ImagePath
        };
    }
}