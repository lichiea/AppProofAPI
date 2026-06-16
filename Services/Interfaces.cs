// Services/Interfaces.cs
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ProofAPI.Models;

namespace ProofAPI.Services
{
    public interface IDataImportService
    {
        Task<ApiSpec> ImportOpenApiAsync(string filePath);
    }

    public interface ILoadTestService
    {
        Task<LoadTestMetric> RunLoadTestAsync(ApiSpec spec, int virtualUsers, int durationSeconds, int rampUpSeconds);
    }

    public interface ISecurityTestService
    {
        Task<List<Vulnerability>> RunSecurityTestsAsync(ApiSpec spec);
    }

    public interface IAuthService
    {
        void SetBasicAuth(string username, string password);
        void SetBearerToken(string token);
        Task AddAuthHeaderAsync(HttpRequestMessage request);
    }

    public interface IDatabaseService
    {
        Task SaveTestRunAsync(Project project, TestRun testRun);
        Task<List<TestRun>> GetHistoryAsync(int projectId);
    }

    public interface IReportGenerator
    {
        Task<string> GenerateHtmlReportAsync(TestRun testRun);
    }
}