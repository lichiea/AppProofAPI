using System.Collections.ObjectModel;

namespace AppProofAPI.ViewModels;

public class CollectionNode
{
    public string? Name { get; set; }
    public ObservableCollection<CollectionNode> Children { get; set; } = new();
}