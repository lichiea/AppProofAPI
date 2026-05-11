using ProofAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace ProofAPI.Services
{

    public interface IDataExporter
    {
        Task ExportToJsonAsync(object data, string filePath);
    }

}