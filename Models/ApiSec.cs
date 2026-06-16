// Models/ApiSpec.cs (внутреннее представление OpenAPI)
using System.Collections.Generic;

namespace ProofAPI.Models
{
    public class ApiEndpoint
    {
        public string Path { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty; // GET, POST, ...
        public Dictionary<string, string> Parameters { get; set; } = new(); // paramName -> example value
    }

    public class ApiSpec
    {
        public List<ApiEndpoint> Endpoints { get; set; } = new();
        public string? BaseUrl { get; set; }
    }
}