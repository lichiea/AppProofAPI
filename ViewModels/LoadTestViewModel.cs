using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using ProofAPI.Models;
using ProofAPI.Services;

namespace AppProofAPI.ViewModels;

public partial class LoadTestViewModel : ObservableObject
{
    private readonly TestOrchestrator _orchestrator;

    [ObservableProperty]
    private int _virtualUsers = 50;

    [ObservableProperty]
    private int _durationSeconds = 60;

    [ObservableProperty]
    private int _rampUpSeconds = 10;

    [ObservableProperty]
    private string _status = "Готов";

    [ObservableProperty]
    private string _verdict = "";

    [ObservableProperty]
    private string _summary = "";

    [ObservableProperty]
    private bool _isRunning = false;

    public LoadTestViewModel(TestOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    [RelayCommand]
    private async Task RunTest()
    {
        if (IsRunning) return;
        IsRunning = true;
        Status = "Запуск теста...";

        try
        {
            // В реальном проекте ApiSpec должен быть загружен из ImportViewModel
            // Для демонстрации создадим фиктивный spec
            var spec = new ApiSpec
            {
                BaseUrl = "https://api.example.com",
                Endpoints = new System.Collections.Generic.List<ApiEndpoint>
                {
                    new ApiEndpoint { Path = "/v1/users", Method = "GET", Parameters = new() }
                }
            };
            var project = new Project { Name = "Test Project" };

            var result = await _orchestrator.RunFullTestAsync(project, spec, VirtualUsers, DurationSeconds, RampUpSeconds);

            Verdict = result.Verdict;
            Summary = result.Summary;
            Status = $"Тест завершён. Вердикт: {result.Verdict}";
        }
        catch (Exception ex)
        {
            Status = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsRunning = false;
        }
    }
}