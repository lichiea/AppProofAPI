using ProofAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace ProofAPI.Services
{
    public class SecurityTestService : ISecurityTestService
    {
        public async Task<List<Vulnerability>> RunSecurityScanAsync(ApiSpec spec)
        {
            // Заглушка: найденные уязвимости
            await Task.Delay(300);
            return new List<Vulnerability>
            {
                new Vulnerability { Type = "SQLi", Endpoint = "/users", Payload = "' OR '1'='1", Evidence = "SQL syntax error" }
            };
        }
    }
}