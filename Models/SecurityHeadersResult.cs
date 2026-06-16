// Models/SecurityHeadersResult.cs
namespace ProofAPI.Models
{
    public class SecurityHeadersResult
    {
        public int Id { get; set; }
        public int TestRunId { get; set; }
        public string HeaderName { get; set; } = string.Empty;
        public bool IsPresent { get; set; }
        public bool IsCorrect { get; set; }
        public string ExpectedValue { get; set; } = string.Empty;
        public string ActualValue { get; set; } = string.Empty;

        public TestRun TestRun { get; set; } = null!;
    }
}