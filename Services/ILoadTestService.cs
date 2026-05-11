using ProofAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace ProofAPI.Services
{
    public interface ILoadTestService
    {
        Task<LoadTestMetric> RunLoadTestAsync(ApiSpec spec, int virtualUsers, int durationSeconds);
    }

}