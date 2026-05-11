using Avalonia.Controls;
using AppProofAPI.ViewModels;

namespace AppProofAPI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}