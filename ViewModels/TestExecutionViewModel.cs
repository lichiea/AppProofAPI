using System;
using System.Collections.ObjectModel;
using System.Net.Http; // ДОБАВЛЕНО для HttpRequestException
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProofAPI.Services;
using ProofAPI.Models;

namespace AppProofAPI.ViewModels
{
    public partial class TestExecutionViewModel : ObservableObject
    {
        private readonly TestOrchestrator _orchestrator;
        private readonly DatabaseService _dbService;
        private readonly DataImportService _importService;

        // ДОБАВЛЕНО: поле для кэширования спецификации
        private ApiSpec? _currentSpec;

        [ObservableProperty] private string _projectName = "Выберите проект";
        [ObservableProperty] private ObservableCollection<Project> _availableProjects = new();
        [ObservableProperty] private Project? _selectedProject;

        [ObservableProperty] private int _vuCount = 50;
        [ObservableProperty] private int _durationSec = 60;
        [ObservableProperty] private int _rampUpSec = 10;

        [ObservableProperty] private double _loadTestProgress;
        [ObservableProperty] private double _securityTestProgress;
        [ObservableProperty] private string _loadTestStatus = "Ожидание запуска...";
        [ObservableProperty] private string _securityTestStatus = "Ожидание запуска...";

        [ObservableProperty] private int _totalRequests;
        [ObservableProperty] private double _currentRPS;
        [ObservableProperty] private int _avgResponseTime;
        [ObservableProperty] private int _errorCount;
        [ObservableProperty] private int _checkedEndpoints;
        [ObservableProperty] private int _foundVulnerabilities;

        [ObservableProperty] private string _elapsedTime = "00:00:00";
        [ObservableProperty] private bool _isRunning;
        [ObservableProperty] private ObservableCollection<string> _executionLogs = new();

        public TestExecutionViewModel(TestOrchestrator orchestrator, DatabaseService dbService, DataImportService importService)
        {
            _orchestrator = orchestrator;
            _dbService = dbService;
            _importService = importService;

            _ = LoadProjectsAsync();
        }

        private async Task LoadProjectsAsync()
        {
            try
            {
                var projects = await _dbService.GetAllProjectsAsync();
                AvailableProjects = new ObservableCollection<Project>(projects);

                if (projects.Count > 0)
                {
                    SelectedProject = projects[0];
                    ProjectName = SelectedProject.Name;
                }
            }
            catch (Exception ex)
            {
                AddLog($"[ОШИБКА] Не удалось загрузить проекты: {ex.Message}");
            }
        }

        partial void OnSelectedProjectChanged(Project? value)
        {
            if (value != null)
            {
                ProjectName = value.Name;
                _currentSpec = null; // Сбрасываем кэш при смене проекта
                AddLog($"[МУП] Выбран проект: {value.Name}");
            }
        }

        [RelayCommand]
        private async Task StartTestAsync()
        {
            if (IsRunning || SelectedProject == null)
            {
                AddLog("[ОШИБКА] Выберите проект перед запуском теста");
                return;
            }

            IsRunning = true;
            AddLog($"[МУП] Запуск комплексного тестирования для проекта '{SelectedProject.Name}'...");
            AddLog($"[МУП] Параметры: VU={VuCount}, Duration={DurationSec}s, RampUp={RampUpSec}s");

            var startTime = DateTime.UtcNow;
            _ = Task.Run(async () =>
            {
                while (IsRunning)
                {
                    var elapsed = DateTime.UtcNow - startTime;
                    ElapsedTime = elapsed.ToString(@"hh\:mm\:ss");
                    await Task.Delay(1000);
                }
            });

            try
            {
                // Загрузка спецификации API из файла проекта
                AddLog("[МОДП] Загрузка спецификации OpenAPI...");
                _currentSpec = await _importService.ImportOpenApiAsync(SelectedProject.OpenApiFilePath);

    if (string.IsNullOrWhiteSpace(_currentSpec.BaseUrl))
    {
        AddLog("[ОШИБКА] BaseUrl пустой! Проверьте секцию 'servers' в OpenAPI спецификации.");
        IsRunning = false;
        return;
    }

                AddLog($"[МОДП] Загружено {_currentSpec.Endpoints.Count} эндпоинтов. URL: {_currentSpec.BaseUrl}");

                LoadTestStatus = "Инициализация МНТ...";
                SecurityTestStatus = "Инициализация МТБ...";

                AddLog("[МУП] Запуск параллельных потоков тестирования...");

                var testRun = await _orchestrator.RunFullTestAsync(
                    SelectedProject, _currentSpec, VuCount, DurationSec, RampUpSec);

                LoadTestProgress = 100;
                SecurityTestProgress = 100;
                LoadTestStatus = "✓ Завершено";
                SecurityTestStatus = "✓ Завершено";

                AddLog($"[МУП] Тестирование завершено. Вердикт: {testRun.Verdict}");
                AddLog($"[МУТ] {testRun.Summary}");
            }
            catch (TaskCanceledException)
            {
                AddLog("[МУП] Тест завершен (время истекло)");
                LoadTestStatus = "⏱ Завершено по времени";
                SecurityTestStatus = "⏱ Завершено по времени";
            }
            catch (OperationCanceledException)
            {
                AddLog("[МУП] Тест остановлен пользователем");
                LoadTestStatus = "⏹ Остановлено";
                SecurityTestStatus = "⏹ Остановлено";
            }
            catch (HttpRequestException httpEx)
            {
                AddLog($"[ОШИБКА] Сетевая ошибка: {httpEx.Message}");
                AddLog("[МУП] Проверьте доступность API и сетевое подключение");
                LoadTestStatus = "✗ Ошибка сети";
                SecurityTestStatus = "✗ Ошибка сети";
            }
            catch (Exception ex)
            {
                AddLog($"[ОШИБКА] {ex.GetType().Name}: {ex.Message}");
                LoadTestStatus = "✗ Ошибка";
                SecurityTestStatus = "✗ Ошибка";
            }
            finally
            {
                IsRunning = false;
            }
        }

        [RelayCommand]
        private void Stop()
        {
            AddLog("[МУП] Остановка тестирования по команде пользователя...");
            IsRunning = false;
        }

        private void AddLog(string message)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                ExecutionLogs.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            });
        }
    }
}