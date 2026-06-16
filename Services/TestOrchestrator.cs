using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProofAPI.Models;

namespace ProofAPI.Services
{
    public class TestOrchestrator
    {
        private readonly ILoadTestService _loadTest;
        private readonly ISecurityTestService _securityTest;
        private readonly TestManager _testManager;
        private readonly IDatabaseService _db;
        private readonly IReportGenerator _reportGen;

        public TestOrchestrator(ILoadTestService loadTest, ISecurityTestService securityTest,
                                TestManager testManager, IDatabaseService db, IReportGenerator reportGen)
        {
            _loadTest = loadTest;
            _securityTest = securityTest;
            _testManager = testManager;
            _db = db;
            _reportGen = reportGen;
        }

        public async Task<TestRun> RunFullTestAsync(Project project, ApiSpec spec,
            int vuCount, int durationSec, int rampUpSec)
        {
            var testRun = new TestRun
            {
                Project = project,
                StartedAt = DateTime.UtcNow,
                Vulnerabilities = new List<Vulnerability>()
            };

            // Запуск нагрузочного теста и теста безопасности параллельно
            var loadTask = _loadTest.RunLoadTestAsync(spec, vuCount, durationSec, rampUpSec);
            var secTask = _securityTest.RunSecurityTestsAsync(spec);

            await Task.WhenAll(loadTask, secTask);

            var loadMetric = await loadTask;
            var vulnerabilities = await secTask;

            // Агрегация результатов
            var suiteResult = _testManager.AggregateResults(loadMetric, vulnerabilities);
            testRun.Verdict = suiteResult.Verdict;
            testRun.Summary = suiteResult.Summary;
            testRun.LoadTestResultJson = System.Text.Json.JsonSerializer.Serialize(suiteResult.LoadMetric);
            testRun.Vulnerabilities = suiteResult.Vulnerabilities;
            testRun.FinishedAt = DateTime.UtcNow;

            // Сохранение в БД
            await _db.SaveTestRunAsync(project, testRun);

            // Генерация отчёта
            var reportPath = await _reportGen.GenerateHtmlReportAsync(testRun);
            Console.WriteLine($"Report saved to {reportPath}");

            return testRun;
        }
    }
}