using System.Collections.Generic;
using System.Linq;
using ProofAPI.Models;

namespace ProofAPI.Services
{
    public class TestManager
    {
        public TestSuiteResult AggregateResults(LoadTestMetric loadMetric, List<Vulnerability> vulnerabilities)
        {
            var result = new TestSuiteResult();
            result.LoadMetric = loadMetric;
            result.Vulnerabilities = vulnerabilities;

            // Вердикт: FAILED если есть Critical/High уязвимости, WARNING если Medium, иначе PASSED
            if (vulnerabilities.Any(v => v.Severity == "Critical" || v.Severity == "High"))
                result.Verdict = "FAILED";
            else if (vulnerabilities.Any(v => v.Severity == "Medium"))
                result.Verdict = "WARNING";
            else
                result.Verdict = "PASSED";

            result.Summary = $"Тест завершён. Найдено уязвимостей: {vulnerabilities.Count}. " +
                             $"Запросов: {loadMetric.TotalRequests}, ошибок: {loadMetric.ErrorCount}";
            return result;
        }
    }

    public class TestSuiteResult
    {
        public string Verdict { get; set; } = "UNKNOWN";
        public string Summary { get; set; } = string.Empty;
        public LoadTestMetric LoadMetric { get; set; } = new();
        public List<Vulnerability> Vulnerabilities { get; set; } = new();
    }
}