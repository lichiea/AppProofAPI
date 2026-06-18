using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using AppProofAPI.ViewModels;
using System.Linq;

namespace AppProofAPI.Views;

public partial class ImportView : UserControl
{
    public ImportView()
    {
        InitializeComponent();
    }

    // Удобное свойство для доступа к ViewModel
    private ImportViewModel? ViewModel => DataContext as ImportViewModel;

    // Обработчик клика по кнопке "Выбрать файл..."
    private async void OnSelectFileClick(object? sender, RoutedEventArgs e)
    {
        // Получаем корневое окно для вызова системного диалога
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите файл OpenAPI",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("OpenAPI Specification")
                {
                    Patterns = new[] { "*.json", "*.yaml", "*.yml" }
                }
            }
        });

        if (files.Count >= 1)
        {
            // Получаем локальный путь к файлу
            var path = files[0].TryGetLocalPath();
            if (path != null && ViewModel != null)
            {
                // Вызываем метод ViewModel для установки пути и авто-имени
                ViewModel.SetFilePath(path);
            }
        }
    }
}