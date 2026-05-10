using ProofAPI.Models;
namespace ProofAPI.Services
{
    public interface ILoadTestService
    {
        Task<LoadTestMetric> RunLoadTestAsync(ApiSpec spec, int virtualUsers, int durationSeconds);
    }
    public class LoadTestService : ILoadTestService
    {
        public async Task<LoadTestMetric> RunLoadTestAsync(ApiSpec spec, int virtualUsers, int durationSeconds)
        {
            // Заглушка: имитация нагрузки
            await Task.Delay(500);
            return new LoadTestMetric { TotalRequests = 1200, AvgResponseTimeMs = 45.2, P95ResponseTimeMs = 87, ErrorCount = 2 };
        }
    }
}