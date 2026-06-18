using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AppProofAPI.Views;

public partial class ConfirmDeleteWindow : Window
{
    private bool _result = false;

    public ConfirmDeleteWindow(string projectName)
    {
        InitializeComponent();
        this.FindControl<TextBlock>("MessageText")!.Text =
            $"Удалить проект \"{projectName}\" и всю историю его тестов?\nЭто действие необратимо.";
    }

    private void ConfirmBtn_Click(object? sender, RoutedEventArgs e)
    {
        _result = true;
        Close(_result);
    }

    private void CancelBtn_Click(object? sender, RoutedEventArgs e)
    {
        _result = false;
        Close(_result);
    }
}