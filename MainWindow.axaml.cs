using Avalonia.Controls;
using AppProofAPI.ViewModels;
using ProofAPI.Models;
using System.Threading.Tasks;

namespace AppProofAPI; // <-- Должно совпадать с x:Class в XAML!

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        if (DataContext is MainWindowViewModel vm)
        {
            vm.AskDeleteConfirmation += ShowDeleteConfirmationAsync;
        }
        else
        {
            this.AttachedToVisualTree += (s, e) =>
            {
                if (DataContext is MainWindowViewModel vmAttached)
                    vmAttached.AskDeleteConfirmation += ShowDeleteConfirmationAsync;
            };
        }
    }

    private async Task<bool> ShowDeleteConfirmationAsync(Project project)
    {
        var dialog = new Views.ConfirmDeleteWindow(project.Name);
        bool result = await dialog.ShowDialog<bool>(this);
        return result;
    }
}