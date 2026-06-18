using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ProofAPI.Models;

namespace ProofAPI.Services
{
    public class LoadTestService : ILoadTestService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthService _authService;

        public LoadTestService(IAuthService authService)
        {
            // Используем SocketsHttpHandler для предотвращения Socket Exhaustion
            var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(10),
                MaxConnectionsPerServer = 100
            };
            _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
            _authService = authService;
        }

        public async Task<LoadTestMetric> RunLoadTestAsync(ApiSpec spec, int virtualUsers, int durationSeconds, int rampUpSeconds)
        {
            System.Diagnostics.Debug.WriteLine($"[МНТ] Запуск нагрузочного теста: VU={virtualUsers}, Duration={durationSeconds}s");
            System.Diagnostics.Debug.WriteLine($"[МНТ] BaseUrl: {spec.BaseUrl}");
            System.Diagnostics.Debug.WriteLine($"[МНТ] Количество эндпоинтов: {spec.Endpoints.Count}");

            if (spec.Endpoints.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[МНТ] ОШИБКА: Нет эндпоинтов для тестирования!");
                return new LoadTestMetric();
            }

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(durationSeconds));
            var tasks = new List<Task<VirtualUserStats>>();

            // Постепенный запуск пользователей (ramp-up)
            int usersPerStep = Math.Max(1, virtualUsers / Math.Max(1, rampUpSeconds));
            int started = 0;

            for (int sec = 0; sec < rampUpSeconds && started < virtualUsers; sec++)
            {
                int toStart = Math.Min(usersPerStep, virtualUsers - started);
                for (int i = 0; i < toStart; i++)
                {
                    tasks.Add(RunVirtualUser(spec, cts.Token));
                }
                started += toStart;
                await Task.Delay(1000);
            }

            // Ожидаем завершения всех VU
            var results = await Task.WhenAll(tasks);

            // Агрегация метрик
            var metric = new LoadTestMetric
            {
                TotalRequests = results.Sum(r => r.Requests),
                ErrorCount = results.Sum(r => r.Errors),
                AvgResponseTimeMs = results.Length > 0 ? results.Average(r => r.AvgResponseMs) : 0,
                MaxResponseTimeMs = results.Length > 0 ? results.Max(r => r.MaxResponseMs) : 0,
                MinResponseTimeMs = results.Length > 0 ? results.Min(r => r.MinResponseMs) : 0,
                RequestsPerSecond = results.Sum(r => r.Requests) / (double)durationSeconds
            };

            return metric;
        }

        private async Task<VirtualUserStats> RunVirtualUser(ApiSpec spec, CancellationToken ct)
        {
            var stats = new VirtualUserStats();
            var random = new Random();
            var stopwatch = new Stopwatch();

            while (!ct.IsCancellationRequested)
            {
                var endpoint = spec.Endpoints[random.Next(spec.Endpoints.Count)];

                // ПРАВИЛЬНОЕ ФОРМИРОВАНИЕ URL (убираем двойные слеши)
                var baseUrl = spec.BaseUrl.TrimEnd('/');
                var path = endpoint.Path.TrimStart('/');
                var url = $"{baseUrl}/{path}";

                var request = new HttpRequestMessage(new HttpMethod(endpoint.Method), url);
                await _authService.AddAuthHeaderAsync(request);

                stopwatch.Restart();
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[МНТ] Отправка запроса: {url}");
                    var response = await _httpClient.SendAsync(request, ct);
                    stopwatch.Stop();

                    stats.Requests++;
                    stats.UpdateResponseTime(stopwatch.ElapsedMilliseconds);

                    if (!response.IsSuccessStatusCode)
                    {
                        System.Diagnostics.Debug.WriteLine($"[МНТ] Ошибка {(int)response.StatusCode}: {url}");
                        stats.Errors++;
                    }
                }
                catch (TaskCanceledException)
                {
                    System.Diagnostics.Debug.WriteLine($"[МНТ] Запрос отменен по таймауту: {url}");
                    break; // Корректный выход из цикла
                }
                catch (HttpRequestException httpEx)
                {
                    stopwatch.Stop();
                    System.Diagnostics.Debug.WriteLine($"[МНТ] HttpRequestException: {httpEx.Message}");
                    stats.Errors++;
                }
                catch (InvalidOperationException invEx)
                {
                    stopwatch.Stop();
                    System.Diagnostics.Debug.WriteLine($"[МНТ] InvalidOperationException: {invEx.Message}");
                    stats.Errors++;
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    System.Diagnostics.Debug.WriteLine($"[МНТ] Exception: {ex.Message}");
                    stats.Errors++;
                }

                // Пауза между запросами (имитация think time)
                try
                {
                    await Task.Delay(100, ct);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

            System.Diagnostics.Debug.WriteLine($"[МНТ] VU завершен. Запросов: {stats.Requests}, Ошибок: {stats.Errors}");
            return stats;
        }

        // ВНУТРЕННИЙ КЛАСС ДЛЯ СБОРА СТАТИСТИКИ ВИРТУАЛЬНОГО ПОЛЬЗОВАТЕЛЯ
        private class VirtualUserStats
        {
            public int Requests { get; set; }
            public int Errors { get; set; }
            public long TotalResponseMs { get; private set; }
            public double AvgResponseMs => Requests == 0 ? 0 : (double)TotalResponseMs / Requests;
            public double MaxResponseMs { get; private set; }
            public double MinResponseMs { get; private set; } = double.MaxValue;

            public void UpdateResponseTime(long ms)
            {
                TotalResponseMs += ms;
                if (ms > MaxResponseMs) MaxResponseMs = ms;
                if (ms < MinResponseMs) MinResponseMs = ms;
            }
        }
    }
}