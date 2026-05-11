using Avalonia.Controls;
using AppProofAPI.ViewModels;

namespace AppProofAPI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var vm = new MainWindowViewModel();
        DataContext = vm;
        vm.SetWindow(this); // передаём ссылку
    }
}