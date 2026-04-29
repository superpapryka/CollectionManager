using CollectionManager.Models;
using System.Collections.ObjectModel;

namespace CollectionManager.Views;

public partial class CollectionPage : ContentPage
{
    private readonly CollectionDefinition _collection;
    private readonly ObservableCollection<CollectibleItem> _items = new();
    private CollectibleItem? _selectedItem;

    public CollectionPage(CollectionDefinition collection)
    {
        InitializeComponent();

        _collection = collection;
        Title = collection.Name;

        CollectionNameLabel.Text = collection.Name;
        DescriptionLabel.Text = collection.Description;

        ItemsView.ItemsSource = _items;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var loaded = await App.Store.LoadCollectionFromFileAsync(_collection.FilePath);

        ClearSelection();
        _items.Clear();

        var sorted = loaded.Items
            .OrderBy(i => GetStatusPriority(i.Status.ToString()))
            .ThenBy(i => i.Name);

        foreach (var item in sorted)
        {
            item.IsSelected = false;
            _items.Add(item);
        }

        CountLabel.Text = $"Items: {_items.Count}";
    }

    private int GetStatusPriority(string status)
    {
        return status switch
        {
            "Owned" => 0,
            "Sold" => 1,
            "WantToSell" => 2,
            _ => 99
        };
    }

    private void ClearSelection()
    {
        if (_selectedItem != null)
            _selectedItem.IsSelected = false;

        _selectedItem = null;
    }

    private void Item_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is not BindableObject bindable)
            return;

        if (bindable.BindingContext is not CollectibleItem tapped)
            return;

        if (_selectedItem == tapped)
        {
            tapped.IsSelected = false;
            _selectedItem = null;
            return;
        }

        if (_selectedItem != null)
            _selectedItem.IsSelected = false;

        tapped.IsSelected = true;
        _selectedItem = tapped;
    }

    private async void AddItem_Clicked(object sender, EventArgs e)
    {
        await OpenEditorAsync(null);
    }

    private async void EditItem_Clicked(object sender, EventArgs e)
    {
        var selected = _selectedItem;
        if (selected is null)
        {
            await DisplayAlert("Info", "Select an item first.", "OK");
            return;
        }

        await OpenEditorAsync(Clone(selected));
    }

    private async void DeleteItem_Clicked(object sender, EventArgs e)
    {
        var selected = _selectedItem;
        if (selected is null)
        {
            await DisplayAlert("Info", "Select an item first.", "OK");
            return;
        }

        var confirm = await DisplayAlert("Delete", $"Delete '{selected.Name}'?", "Yes", "No");
        if (!confirm)
            return;

        _items.Remove(selected);
        ClearSelection();

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
            ImagePath = source.ImagePath,
            IsSelected = false
        };
    }
}