using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using ProofAPI.Services;
using ProofAPI.Models;

namespace AppProofAPI.ViewModels;

public partial class ReportViewModel : ObservableObject
{
    private readonly IReportGenerator _reportGen;
    private readonly IDatabaseService _db;

    [ObservableProperty]
    private string _reportPath = "";

    [ObservableProperty]
    private string _status = "Готов к генерации";

    public ReportViewModel(IReportGenerator reportGen, IDatabaseService db)
    {
        _reportGen = reportGen;
        _db = db;
    }

    [RelayCommand]
    private async Task GenerateReport()
    {
        try
        {
            Status = "Генерация отчёта...";
            // В реальном проекте нужно выбрать существующий TestRun из БД
            // Для демонстрации создадим фиктивный объект
            var testRun = new TestRun
            {
                Id = 1,
                StartedAt = DateTime.Now.AddMinutes(-5),
                FinishedAt = DateTime.Now,
                Verdict = "FAILED",
                Summary = "Найдено 3 критических уязвимости",
                Vulnerabilities = new System.Collections.Generic.List<Vulnerability>
                {
                    new Vulnerability
                    {
                        Type = "SQL Injection",
                        Severity = "Critical",
                        Endpoint = "/login",
                        Payload = "' OR 1=1 --",
                        Evidence = "Error: syntax error near 'OR'",
                        DetectedAt = DateTime.Now
                    }
                }
            };
            var path = await _reportGen.GenerateHtmlReportAsync(testRun);
            ReportPath = path;
            Status = $"Отчёт сохранён: {path}";
        }
        catch (Exception ex)
        {
            Status = $"Ошибка: {ex.Message}";
        }
    }
}