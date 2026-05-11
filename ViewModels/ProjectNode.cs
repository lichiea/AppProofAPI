using System.Collections.ObjectModel;

namespace AppProofAPI.ViewModels;

public class ProjectNode
{
    public string? Name { get; set; }
    public ObservableCollection<ProjectNode> Children { get; set; } = new();
}