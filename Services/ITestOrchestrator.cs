using ProofAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace ProofAPI.Services
{
    public interface ITestOrchestrator
    {
        Task<(LoadTestMetric loadResult, List<Vulnerability> vulns)> RunAllTestsAsync(ApiSpec spec);
    }
}