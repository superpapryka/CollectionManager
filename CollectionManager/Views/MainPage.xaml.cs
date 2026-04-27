using CollectionManager.Models;

namespace CollectionManager.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCollectionsAsync();
    }

    private async Task LoadCollectionsAsync()
    {
        var collections = await App.Store.GetCollectionsAsync();
        CollectionsView.ItemsSource = collections;
    }

    private async void Refresh_Clicked(object sender, EventArgs e)
    {
        await LoadCollectionsAsync();
    }

    private async void AddCollection_Clicked(object sender, EventArgs e)
    {
        var name = await DisplayPromptAsync("New Collection", "Collection name:");
        if (string.IsNullOrWhiteSpace(name))
            return;

        var trimmedName = name.Trim();

        var existingCollections = CollectionsView.ItemsSource as IEnumerable<CollectionDefinition>;
        bool duplicate = existingCollections != null &&
                         existingCollections.Any(x => x.Name.Equals(trimmedName, StringComparison.OrdinalIgnoreCase));

        if (duplicate)
        {
            var confirm = await DisplayAlert(
                "Duplicate name",
                "A collection with this name already exists. Create anyway?",
                "Yes",
                "No");

            if (!confirm)
                return;
        }

        var description = await DisplayPromptAsync("New Collection", "Description:") ?? string.Empty;

        await App.Store.CreateCollectionAsync(trimmedName, description.Trim());
        await LoadCollectionsAsync();
    }

    private async void CollectionsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = e.CurrentSelection.FirstOrDefault() as CollectionDefinition;
        if (selected is null)
            return;

        ((CollectionView)sender).SelectedItem = null;
        await Navigation.PushAsync(new CollectionPage(selected));
    }
}