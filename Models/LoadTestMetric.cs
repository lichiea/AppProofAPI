// Файл: Models/LoadTestMetric.cs
using System.Collections.Generic;

namespace ProofAPI.Models
{
    public class LoadTestMetric
    {
        public int TotalRequests { get; set; }
        public int ErrorCount { get; set; }
        public double AvgResponseTimeMs { get; set; }
        public double MaxResponseTimeMs { get; set; }
        public double MinResponseTimeMs { get; set; }

        // НОВЫЕ ПОЛЯ ДЛЯ ВКР (Перцентили и статус-коды)
        public double P50ResponseTimeMs { get; set; }
        public double P95ResponseTimeMs { get; set; }
        public double P99ResponseTimeMs { get; set; }
        public Dictionary<int, int> StatusCodes { get; set; } = new();

        public double RequestsPerSecond { get; set; }
    }
}