namespace CollectionManager.Models;

public class CollectionDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}