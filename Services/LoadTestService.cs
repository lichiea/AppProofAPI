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
            _httpClient = new HttpClient();
            _authService = authService;
        }

        public async Task<LoadTestMetric> RunLoadTestAsync(ApiSpec spec, int virtualUsers, int durationSeconds, int rampUpSeconds)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(durationSeconds));
            var tasks = new List<Task<VirtualUserStats>>();

            // Постепенный запуск пользователей (ramp-up)
            int usersPerStep = Math.Max(1, virtualUsers / rampUpSeconds);
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
                AvgResponseTimeMs = results.Average(r => r.AvgResponseMs),
                MaxResponseTimeMs = results.Max(r => r.MaxResponseMs),
                MinResponseTimeMs = results.Min(r => r.MinResponseMs),
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
                var url = spec.BaseUrl + endpoint.Path;
                var request = new HttpRequestMessage(new HttpMethod(endpoint.Method), url);
                await _authService.AddAuthHeaderAsync(request);

                stopwatch.Restart();
                try
                {
                    var response = await _httpClient.SendAsync(request, ct);
                    stopwatch.Stop();
                    stats.Requests++;
                    stats.UpdateResponseTime(stopwatch.ElapsedMilliseconds);
                    if (!response.IsSuccessStatusCode)
                        stats.Errors++;
                }
                catch
                {
                    stats.Errors++;
                }

                await Task.Delay(100, ct); // пауза между запросами
            }
            return stats;
        }

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