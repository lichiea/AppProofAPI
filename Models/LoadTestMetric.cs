// Models/LoadTestMetric.cs
namespace ProofAPI.Models
{
    public class LoadTestMetric
    {
        public int TotalRequests { get; set; }
        public int ErrorCount { get; set; }
        public double AvgResponseTimeMs { get; set; }
        public double MaxResponseTimeMs { get; set; }
        public double MinResponseTimeMs { get; set; }
        public double RequestsPerSecond { get; set; }
    }
}