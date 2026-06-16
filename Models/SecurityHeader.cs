namespace ProofAPI.Models
{
    public class SecurityHeader
    {
        public string? Name { get; set; }
        public bool IsPresent { get; set; }
        public bool IsValid { get; set; }
        public string? Expected { get; set; }
        public string? Actual { get; set; }
    }
}