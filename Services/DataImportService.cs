using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OpenApi.Readers;
using ProofAPI.Models;

namespace ProofAPI.Services
{
    public class DataImportService : IDataImportService
    {
        public async Task<ApiSpec> ImportOpenApiAsync(string filePath)
        {
            var fileContent = await File.ReadAllTextAsync(filePath);
            var reader = new OpenApiStringReader();
            var doc = reader.Read(fileContent, out var diagnostic);

            if (diagnostic.Errors.Any())
                throw new Exception($"OpenAPI parsing errors: {string.Join(", ", diagnostic.Errors)}");

            var spec = new ApiSpec();
            spec.BaseUrl = doc.Servers.FirstOrDefault()?.Url ?? "";

            foreach (var path in doc.Paths)
            {
                foreach (var operation in path.Value.Operations)
                {
                    var endpoint = new ApiEndpoint
                    {
                        Path = path.Key,
                        Method = operation.Key.ToString().ToUpper()
                    };

                    // Извлечение примеров параметров (для фаззинга)
                    foreach (var param in operation.Value.Parameters)
                    {
                        endpoint.Parameters[param.Name] = param.Schema?.Example?.ToString() ?? "test";
                    }
                    spec.Endpoints.Add(endpoint);
                }
            }
            return spec;
        }
    }
}