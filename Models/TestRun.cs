// Models/TestRun.cs
using System;
using System.Collections.Generic;

namespace ProofAPI.Models
{
    public class TestRun
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime FinishedAt { get; set; }
        public string Verdict { get; set; } = "UNKNOWN"; // PASSED, WARNING, FAILED
        public string LoadTestResultJson { get; set; } = "{}"; // JSON-сериализованный LoadTestMetric
        public string Summary { get; set; } = string.Empty;

        public Project Project { get; set; } = null!;
        public ICollection<Vulnerability> Vulnerabilities { get; set; } = new List<Vulnerability>();
        public ICollection<SecurityHeadersResult> SecurityHeadersResults { get; set; } = new List<SecurityHeadersResult>();
    }
}