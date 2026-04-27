namespace CollectionManager.Models;

public enum ItemStatus
{
    Owned = 0,
    Sold = 1,
    WantToSell = 2
}

public class CollectibleItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public ItemStatus Status { get; set; } = ItemStatus.Owned;
    public string Notes { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
}