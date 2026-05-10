namespace ProofAPI.Models
{
    public class TestResult
    {
        public string Endpoint { get; set; } = null!;
        public int StatusCode { get; set; }
        public long ResponseTimeMs { get; set; }
        public bool IsPassed { get; set; }
        public string? ErrorMessage { get; set; }
    }
}