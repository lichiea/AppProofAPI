using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace AppProofAPI.ViewModels;

public partial class SecurityTestViewModel : ObservableObject
{
    [ObservableProperty]
    private string _status = "Готов к запуску проверок безопасности";

    [ObservableProperty]
    private bool _isRunning = false;

    [ObservableProperty]
    private string _lastResult = "";

    [ObservableProperty]
    private bool _showErrorSimulation = false;

    [ObservableProperty]
    private string _errorDetails = "";

    public SecurityTestViewModel()
    {
    }

    [RelayCommand]
    private async Task RunSecurityScan()
    {
        if (IsRunning) return;
        IsRunning = true;
        Status = "Выполнение сканирования...";

        // Имитация работы с возможностью ошибки
        await Task.Delay(2000);

        // Случайно имитируем ошибку сети для демонстрации
        var rand = new Random();
        if (rand.Next(0, 3) == 0) // 33% шанс
        {
            ShowErrorSimulation = true;
            ErrorDetails = "System.Net.Http.HttpRequestException: Connection timed out — Network unreachable\n" +
                        "   at System.Net.Http.ConnectHelper.ConnectAsync(...)\n" +
                        "   at ProofAPI.Services.LoadTestService.ExecuteRequestAsync(...)";
            Status = "Обнаружен обрыв соединения. Частичные результаты сохранены.";
        }
        else
        {
            ShowErrorSimulation = false;
            LastResult = "Сканирование завершено. Найдено 0 критических, 2 средних уязвимости.";
            Status = "Сканирование завершено успешно";
        }

        IsRunning = false;
    }
}