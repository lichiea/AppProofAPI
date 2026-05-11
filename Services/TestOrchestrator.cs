using ProofAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace ProofAPI.Services
{
    public class TestOrchestrator : ITestOrchestrator
    {
        private readonly ILoadTestService _loadTest;
        private readonly ISecurityTestService _securityTest;
        public TestOrchestrator(ILoadTestService loadTest, ISecurityTestService securityTest)
        {
            _loadTest = loadTest;
            _securityTest = securityTest;
        }
        public async Task<(LoadTestMetric loadResult, List<Vulnerability> vulns)> RunAllTestsAsync(ApiSpec spec)
        {
            var loadTask = _loadTest.RunLoadTestAsync(spec, virtualUsers: 10, durationSeconds: 30);
            var securityTask = _securityTest.RunSecurityScanAsync(spec);
            await Task.WhenAll(loadTask, securityTask);
            return (await loadTask, await securityTask);
        }
    }
}