using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProofAPI.Models;
using ProofAPI.Services;
using System.Threading.Tasks;

namespace ProofAPI.Desktop.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DataImportService _import;
        private readonly AuthService _auth;
        private readonly LoadTestService _loadTest;
        private readonly SecurityTestService _securityTest;
        private readonly DatabaseService _db;
        private readonly TestOrchestrator _orchestrator;

        [ObservableProperty] private string _status = "Готов";
        [ObservableProperty] private string _verdict;

        public MainViewModel()
        {
            // Инициализация зависимостей (можно через DI)
            _auth = new AuthService();
            _import = new DataImportService();
            _loadTest = new LoadTestService(_auth);
            _securityTest = new SecurityTestService(_auth);
            _db = new DatabaseService();
            _orchestrator = new TestOrchestrator(_loadTest, _securityTest, new TestManager(), _db, new ReportGenerator());
        }

        [RelayCommand]
        private async Task LoadOpenApi(string filePath)
        {
            Status = "Импорт OpenAPI...";
            var spec = await _import.ImportOpenApiAsync(filePath);
            Status = $"Импортировано {spec.Endpoints.Count} эндпоинтов";
        }

        [RelayCommand]
        private async Task RunTest()
        {
            Status = "Запуск теста...";
            var project = new Project { Name = "Test Project" };
            var spec = new ApiSpec(); // должен быть заполнен из импорта
            var result = await _orchestrator.RunFullTestAsync(project, spec, 50, 60, 10);
            Verdict = result.Verdict;
            Status = $"Тест завершён. Вердикт: {result.Verdict}";
        }
    }
}