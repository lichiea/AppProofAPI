using ProofAPI.Models;
namespace ProofAPI.Services
{
    public interface IReportGenerator
    {
        Task<string> GenerateHtmlReportAsync(LoadTestMetric load, List<Vulnerability> vulns);
    }
}