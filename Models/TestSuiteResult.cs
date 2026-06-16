using System.Collections.Generic;

namespace ProofAPI.Models
{
    public class TestSuiteResult
    {
        internal object? StartedAt;
        internal object? FinishedAt;

        public string? Verdict { get; set; }
        public int? SecurityScore { get; set; }
        public string? SummaryText { get; set; }
        public LoadMetric? LoadMetric { get; set; }
        public List<Vulnerability>? Vulnerabilities { get; set; }
        public List<SecurityHeader>? SecurityHeaders { get; set; }
        public List<CurlCommand>? CurlCommands { get; set; }
        public string? ProofApiVersion { get; set; }
        public string? Platform { get; set; }
        public string? UiFramework { get; set; }
        public string? TargetApi { get; set; }
        public string? ApiSpecification { get; set; }
        public string? Standard { get; set; }
        public string? StandardDetails { get; set; }
    }
}