using System.Collections.Generic;
using ProofAPI.Models;

namespace ProofAPI.Services
{
    public interface ITestManager
    {
        TestSuiteResult AggregateResults(LoadTestMetric loadMetric, List<Vulnerability> vulnerabilities);
    }
}
