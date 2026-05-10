using ProofAPI.Models;
using System.IO;
namespace ProofAPI.Services
{
    public class DataExporter : IDataExporter
    {
        public async Task ExportToJsonAsync(object data, string filePath)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }
    }
}