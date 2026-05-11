using ProofAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace ProofAPI.Services
{
    public interface IDataImportService
    {
        Task<ApiSpec> LoadFromOpenApiAsync(string filePath);
    }
}