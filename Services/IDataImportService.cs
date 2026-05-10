using ProofAPI.Models;
namespace ProofAPI.Services
{
    public interface IDataImportService
    {
        Task<ApiSpec> LoadFromOpenApiAsync(string filePath);
    }
}