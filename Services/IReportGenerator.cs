using ProofAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace ProofAPI.Services
{
    public interface IReportGenerator
    {
        Task<string> GenerateHtmlReportAsync(LoadTestMetric load, List<Vulnerability> vulns);
    }
}