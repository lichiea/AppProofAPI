// Models/TestResult.cs
namespace ProofAPI.Models
{
    public class LoadTestMetric
    {
        public int TotalRequests { get; set; }
        public double AvgResponseTimeMs { get; set; }
        public double P95ResponseTimeMs { get; set; }
        public int ErrorCount { get; set; }
    }
}