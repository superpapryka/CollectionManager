using CollectionManager.Models;

namespace CollectionManager.Views;

public partial class SummaryPage : ContentPage
{
    public SummaryPage(CollectionDefinition collection, IReadOnlyList<CollectibleItem> items)
    {
        InitializeComponent();

        NameLabel.Text = collection.Name;
        DescriptionLabel.Text = collection.Description;

        var totalItems = items.Count;
        var totalQuantity = items.Sum(x => x.Quantity);
        var ownedQuantity = items.Where(x => x.Status == ItemStatus.Owned).Sum(x => x.Quantity);
        var soldQuantity = items.Where(x => x.Status == ItemStatus.Sold).Sum(x => x.Quantity);
        var wantToSellQuantity = items.Where(x => x.Status == ItemStatus.WantToSell).Sum(x => x.Quantity);

        TotalItemsLabel.Text = $"Items on list: {totalItems}";
        TotalQuantityLabel.Text = $"Total quantity: {totalQuantity}";
        OwnedLabel.Text = $"Owned quantity: {ownedQuantity}";
        SoldLabel.Text = $"Sold quantity: {soldQuantity}";
        WantToSellLabel.Text = $"Want to sell quantity: {wantToSellQuantity}";
    }
}