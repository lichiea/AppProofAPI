using ProofAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace ProofAPI.Services
{

    public class ReportGenerator : IReportGenerator
    {
        public async Task<string> GenerateHtmlReportAsync(LoadTestMetric load, List<Vulnerability> vulns)
        {
            var html = $"<html><body><h1>Test Report</h1><p>Load: {load.TotalRequests} requests, avg {load.AvgResponseTimeMs} ms</p><ul>{string.Concat(vulns.Select(v => $"<li>{v.Type} at {v.Endpoint}</li>"))}</ul></body></html>";
            return await Task.FromResult(html);
        }
    }

}