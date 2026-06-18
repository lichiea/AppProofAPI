using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ProofAPI.Models;
using ProofAPI.Services;

namespace AppProofAPI.ViewModels;

public partial class ProjectDetailsViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;

    // Информация о самом проекте
    [ObservableProperty]
    private Project? _project;

    // Список запусков тестов (Отчеты)
    [ObservableProperty]
    private ObservableCollection<TestRun> _testRuns = new();

    // Плоский список всех уязвимостей, найденных во всех запусках этого проекта
    [ObservableProperty]
    private ObservableCollection<Vulnerability> _vulnerabilities = new();

    // Статус загрузки данных
    [ObservableProperty]
    private string _statusMessage = "Загрузка данных...";

    public ProjectDetailsViewModel(DatabaseService dbService, Project initialProject)
    {
        _dbService = dbService;
        Project = initialProject; // Сразу показываем имя, не дожидаясь БД

        // Запускаем асинхронную загрузку полных данных
        _ = LoadProjectDetailsAsync();
    }

    private async Task LoadProjectDetailsAsync()
    {
        try
        {
            // Запрашиваем из БД проект со всеми связями
            var fullProject = await _dbService.GetProjectWithDetailsAsync(Project!.Id);

            if (fullProject != null)
            {
                Project = fullProject; // Обновляем свойство (UI обновится)

                // Сортируем запуски: новые сверху
                TestRuns = new ObservableCollection<TestRun>(
                    fullProject.TestRuns.OrderByDescending(t => t.StartedAt));

                // Собираем все уязвимости из всех запусков в один список
                var allVulns = fullProject.TestRuns
                    .SelectMany(t => t.Vulnerabilities)
                    .OrderByDescending(v => v.DetectedAt);

                Vulnerabilities = new ObservableCollection<Vulnerability>(allVulns);

                StatusMessage = $"Загружено. Запусков: {TestRuns.Count} | Уязвимостей: {Vulnerabilities.Count}";
            }
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Ошибка загрузки данных: {ex.Message}";
        }
    }
}