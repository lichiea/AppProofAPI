using System;
using System.Collections.Generic;

namespace ProofAPI.Models
{
    public class LoadMetric
{
    public int TotalRequests { get; set; }
    public double AvgResponseTimeMs { get; set; }
    public double P95ResponseTimeMs { get; set; }
    public int ErrorCount { get; set; }
    public double ErrorPercent { get; set; }
    public double MaxResponseTimeMs { get; set; }
    public string? SlowestEndpoint { get; set; }
    public double ThroughputMBps { get; set; }
    public Dictionary<int, int>? StatusCodes { get; set; }
    public int Rps { get; set; }

        public static implicit operator LoadMetric(LoadTestMetric v)
        {
            throw new NotImplementedException();
        }
    }
}