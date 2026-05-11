using ProofAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace ProofAPI.Services
{
    public class DataImportService : IDataImportService
    {
        public async Task<ApiSpec> LoadFromOpenApiAsync(string filePath)
        {
            // Здесь будет парсинг OpenAPI (заглушка)
            await Task.Delay(100);
            return new ApiSpec { Title = "Sample API", Endpoints = new List<string> { "/users", "/posts" } };
        }
    }

}