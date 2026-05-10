using ProofAPI.Models;
namespace ProofAPI.Services
{

    public interface IDataExporter
    {
        Task ExportToJsonAsync(object data, string filePath);
    }

}