using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ProofAPI.Models;

namespace ProofAPI.Services
{
    public class ReportGenerator : IReportGenerator
    {
        public async Task<string> GenerateHtmlReportAsync(TestRun testRun)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html><head><title>ProofAPI Report</title></head><body>");
            sb.AppendLine($"<h1>Test Report - {testRun.StartedAt}</h1>");
            sb.AppendLine($"<p>Verdict: <b>{testRun.Verdict}</b></p>");
            sb.AppendLine($"<p>{testRun.Summary}</p>");

            sb.AppendLine("<h2>Vulnerabilities</h2>");
            sb.AppendLine("<table border='1'><tr><th>Type</th><th>Severity</th><th>Endpoint</th><th>Payload</th><th>Evidence</th></tr>");
            foreach (var vuln in testRun.Vulnerabilities)
            {
                sb.AppendLine($"<tr><td>{vuln.Type}</td><td>{vuln.Severity}</td><td>{vuln.Endpoint}</td><td>{vuln.Payload}</td><td>{vuln.Evidence}</td></tr>");
            }
            sb.AppendLine("</table>");

            // cURL команды для воспроизведения (пример)
            sb.AppendLine("<h2>cURL Commands</h2><pre>");
            foreach (var vuln in testRun.Vulnerabilities)
            {
                sb.AppendLine($"# {vuln.Type} at {vuln.Endpoint}");
                sb.AppendLine($"curl -X GET \"{vuln.Endpoint}?test={vuln.Payload}\"");
            }
            sb.AppendLine("</pre>");
            sb.AppendLine("</body></html>");

            var fileName = $"Report_{testRun.StartedAt:yyyyMMdd_HHmmss}.html";
            await File.WriteAllTextAsync(fileName, sb.ToString());
            return fileName;
        }
    }
}