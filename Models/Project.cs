// Models/Project.cs
using System.Collections.Generic;

namespace ProofAPI.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string OpenApiFilePath { get; set; } = string.Empty;
        public ICollection<TestRun> TestRuns { get; set; } = new List<TestRun>();
    }
}