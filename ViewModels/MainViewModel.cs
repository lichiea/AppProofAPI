using System;
using System.Threading.Tasks;
using ProofAPI.Services;
using ProofAPI.Models;

namespace ProofAPI.ViewModels
{
    public class MainViewModel
    {
        private readonly IDataImportService _import;
        private readonly ITestOrchestrator _orchestrator;
        private readonly IReportGenerator _reportGen;
        private ApiSpec _currentSpec = null!;

        public MainViewModel(IDataImportService import, ITestOrchestrator orchestrator, IReportGenerator reportGen)
        {
            _import = import;
            _orchestrator = orchestrator;
            _reportGen = reportGen;
        }

        public async Task LoadApiAsync()
        {
            // В реальности выбрать файл через OpenFileDialog
            _currentSpec = await _import.LoadFromOpenApiAsync("spec.yaml");
        }

        public async Task<string> RunAllTestsAsync()
        {
            if (_currentSpec == null) return "Сначала загрузите спецификацию.";
            var (loadResult, vulns) = await _orchestrator.RunAllTestsAsync(_currentSpec);
            return await _reportGen.GenerateHtmlReportAsync(loadResult, vulns);
        }
    }
}