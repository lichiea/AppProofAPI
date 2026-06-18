using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using ProofAPI.Models;
using ProofAPI.Services;

namespace AppProofAPI.ViewModels;

public partial class ImportViewModel : ObservableObject
{
    private readonly IDataImportService _importService;
    private readonly IDatabaseService _dbService; // Добавляем зависимость от БД

    [ObservableProperty]
    private string _statusMessage = "Готов к импорту";

    [ObservableProperty]
    private ObservableCollection<ApiEndpoint> _endpoints = new();

    [ObservableProperty]
    private ApiSpec? _currentSpec;

    // --- НОВЫЕ СВОЙСТВА ДЛЯ ИНТЕРФЕЙСА ---

    // Название проекта (будет привязано к TextBox)
    [ObservableProperty]
    private string _projectName = "";

    // Путь к выбранному файлу (будет привязан к TextBox и передан в команду)
    [ObservableProperty]
    private string _selectedFilePath = "";

    // Событие для уведомления MainWindow об успешном сохранении (чтобы обновить список слева)
    public event Action? ProjectSavedSuccessfully;

    // Обновляем конструктор, чтобы принимать IDatabaseService
    public ImportViewModel(IDataImportService importService, IDatabaseService dbService)
    {
        _importService = importService;
        _dbService = dbService;
    }

    // Метод, который будет вызван из View после выбора файла в диалоге
    public void SetFilePath(string path)
    {
        SelectedFilePath = path;
        // Автоматически подставляем имя файла без расширения как название проекта
        if (string.IsNullOrWhiteSpace(ProjectName))
        {
            ProjectName = Path.GetFileNameWithoutExtension(path);
        }
        StatusMessage = $"Файл выбран: {Path.GetFileName(path)}";
    }

    // Команда импорта и сохранения
    [RelayCommand]
    private async Task ImportOpenApi(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            StatusMessage = "⚠️ Сначала выберите файл спецификации.";
            return;
        }

        if (string.IsNullOrWhiteSpace(ProjectName))
        {
            StatusMessage = "⚠️ Введите название проекта.";
            return;
        }

        try
        {
            StatusMessage = "⏳ Импорт и парсинг спецификации...";

            // 1. Парсинг спецификации (МОДП)
            var spec = await _importService.ImportOpenApiAsync(filePath);
            CurrentSpec = spec;
            Endpoints.Clear();
            foreach (var ep in spec.Endpoints)
                Endpoints.Add(ep);

            StatusMessage = $"✅ Импортировано {spec.Endpoints.Count} эндпоинтов. Сохранение в БД...";

            // 2. Сохранение проекта в БД (МВБД)
            // Примечание: метод SaveProjectAsync нужно добавить в IDatabaseService и DatabaseService (см. сноску ниже)
            await _dbService.SaveProjectAsync(ProjectName, filePath);

            StatusMessage = $"✅ Проект '{ProjectName}' успешно импортирован и сохранен!";

            // 3. Уведомляем MainWindow, чтобы обновить список проектов в боковой панели
            ProjectSavedSuccessfully?.Invoke();
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка: {ex.Message}";
        }
    }
}