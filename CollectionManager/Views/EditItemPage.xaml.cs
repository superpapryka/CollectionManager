using CollectionManager.Models;

namespace CollectionManager.Views;

public partial class EditItemPage : ContentPage
{
    private readonly CollectionDefinition _collection;
    private readonly Func<CollectibleItem, Task> _onSave;
    private readonly CollectibleItem _item;

    public EditItemPage(
        CollectionDefinition collection,
        CollectibleItem? existingItem,
        Func<CollectibleItem, Task> onSave)
    {
        InitializeComponent();

        _collection = collection;
        _onSave = onSave;

        _item = existingItem is null
            ? new CollectibleItem()
            : new CollectibleItem
            {
                Id = existingItem.Id,
                Name = existingItem.Name,
                Category = existingItem.Category,
                Quantity = existingItem.Quantity,
                Status = existingItem.Status,
                Notes = existingItem.Notes,
                ImagePath = existingItem.ImagePath
            };

        HeaderLabel.Text = existingItem is null ? "Add Item" : "Edit Item";

        NameEntry.Text = _item.Name;
        CategoryEntry.Text = _item.Category;
        QuantityEntry.Text = _item.Quantity.ToString();
        NotesEditor.Text = _item.Notes;
        ImagePathEntry.Text = _item.ImagePath;

        StatusPicker.ItemsSource = Enum.GetNames(typeof(ItemStatus));
        StatusPicker.SelectedIndex = (int)_item.Status;

        UpdatePreview();
        ImagePathEntry.TextChanged += (_, __) => UpdatePreview();
    }

    private async void BrowseImage_Clicked(object sender, EventArgs e)
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Choose an image"
        });

        if (result is null)
            return;

        ImagePathEntry.Text = result.FullPath;
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (!string.IsNullOrWhiteSpace(ImagePathEntry.Text) && File.Exists(ImagePathEntry.Text))
            PreviewImage.Source = ImageSource.FromFile(ImagePathEntry.Text);
        else
            PreviewImage.Source = null;
    }

    private async void Save_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlert("Validation", "Name is required.", "OK");
            return;
        }

        if (!int.TryParse(QuantityEntry.Text, out var quantity) || quantity < 1)
        {
            await DisplayAlert("Validation", "Quantity must be a positive number.", "OK");
            return;
        }

        _item.Name = NameEntry.Text.Trim();
        _item.Category = CategoryEntry.Text?.Trim() ?? string.Empty;
        _item.Quantity = quantity;
        _item.Status = StatusPicker.SelectedIndex >= 0
            ? (ItemStatus)StatusPicker.SelectedIndex
            : ItemStatus.Owned;
        _item.Notes = NotesEditor.Text?.Trim() ?? string.Empty;
        _item.ImagePath = ImagePathEntry.Text?.Trim() ?? string.Empty;

        await _onSave(_item);
        await Navigation.PopAsync();
    }

    private async void Cancel_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}