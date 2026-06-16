using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ProofAPI.Models;

namespace ProofAPI.Services
{
    public class SecurityTestService : ISecurityTestService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthService _authService;

        // Словари полезных нагрузок для фаззинга
        private readonly Dictionary<string, List<string>> _fuzzPayloads = new()
        {
            ["SQLi"] = new() { "' OR '1'='1", "'; DROP TABLE users; --", "1 AND 1=1" },
            ["XSS"] = new() { "<script>alert(1)</script>", "<img src=x onerror=alert(1)>" },
            ["PathTraversal"] = new() { "../../../etc/passwd", "..\\..\\windows\\win.ini" },
            ["SSTI"] = new() { "{{7*7}}", "${7*7}" },
            ["XXE"] = new() { "<?xml version=\"1.0\"?><!DOCTYPE root [<!ENTITY test SYSTEM 'file:///etc/passwd'>]>" }
        };

        private readonly List<string> _securityHeaders = new()
        {
            "Content-Security-Policy", "X-Content-Type-Options", "X-Frame-Options", "Strict-Transport-Security"
        };

        private readonly Regex _piiRegex = new(@"\b\d{4}[- ]?\d{4}[- ]?\d{4}[- ]?\d{4}\b|\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b");

        public SecurityTestService(IAuthService authService)
        {
            _httpClient = new HttpClient();
            _authService = authService;
        }

        public async Task<List<Vulnerability>> RunSecurityTestsAsync(ApiSpec spec)
        {
            var vulnerabilities = new List<Vulnerability>();

            // 1. Пассивная проверка заголовков безопасности
            var headersVulns = await CheckSecurityHeadersAsync(spec.BaseUrl);
            vulnerabilities.AddRange(headersVulns);

            // 2. Активный фаззинг по каждому эндпоинту и параметру
            foreach (var endpoint in spec.Endpoints)
            {
                foreach (var param in endpoint.Parameters)
                {
                    foreach (var attackType in _fuzzPayloads)
                    {
                        foreach (var payload in attackType.Value)
                        {
                            var vuln = await TestFuzzAsync(endpoint, param.Key, payload, attackType.Key);
                            if (vuln != null)
                                vulnerabilities.Add(vuln);
                        }
                    }
                }
            }

            // 3. Поиск утечек PII в ответах (базовые запросы)
            var piiVulns = await CheckPiiLeakageAsync(spec);
            vulnerabilities.AddRange(piiVulns);

            return vulnerabilities;
        }

        private async Task<List<Vulnerability>> CheckSecurityHeadersAsync(string baseUrl)
        {
            var vulns = new List<Vulnerability>();
            var request = new HttpRequestMessage(HttpMethod.Get, baseUrl);
            await _authService.AddAuthHeaderAsync(request);
            var response = await _httpClient.SendAsync(request);

            foreach (var header in _securityHeaders)
            {
                if (!response.Headers.Contains(header))
                {
                    vulns.Add(new Vulnerability
                    {
                        Type = "MissingSecurityHeader",
                        Severity = "Medium",
                        Endpoint = baseUrl,
                        Payload = "",
                        Evidence = $"Header '{header}' is missing.",
                        DetectedAt = DateTime.UtcNow
                    });
                }
            }
            return vulns;
        }

        private async Task<Vulnerability?> TestFuzzAsync(ApiEndpoint endpoint, string paramName, string payload, string attackType)
        {
            var url = $"{endpoint.Path}?{paramName}={Uri.EscapeDataString(payload)}";
            var request = new HttpRequestMessage(new HttpMethod(endpoint.Method), url);
            await _authService.AddAuthHeaderAsync(request);

            try
            {
                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                // Признак уязвимости: ошибка SQL, отображение payload в ответе и т.д.
                if (attackType == "SQLi" && (body.Contains("SQL syntax") || body.Contains("mysql_fetch")))
                {
                    return new Vulnerability
                    {
                        Type = attackType,
                        Severity = "High",
                        Endpoint = endpoint.Path,
                        Payload = payload,
                        Evidence = body.Length > 200 ? body.Substring(0, 200) : body,
                        DetectedAt = DateTime.UtcNow
                    };
                }
                else if (attackType == "XSS" && body.Contains(payload))
                {
                    return new Vulnerability
                    {
                        Type = attackType,
                        Severity = "High",
                        Endpoint = endpoint.Path,
                        Payload = payload,
                        Evidence = body,
                        DetectedAt = DateTime.UtcNow
                    };
                }
            }
            catch { /* игнорируем сетевые ошибки */ }
            return null;
        }

        private async Task<List<Vulnerability>> CheckPiiLeakageAsync(ApiSpec spec)
        {
            var vulns = new List<Vulnerability>();
            foreach (var endpoint in spec.Endpoints)
            {
                var request = new HttpRequestMessage(new HttpMethod(endpoint.Method), spec.BaseUrl + endpoint.Path);
                await _authService.AddAuthHeaderAsync(request);
                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();
                if (_piiRegex.IsMatch(body))
                {
                    vulns.Add(new Vulnerability
                    {
                        Type = "PIILeakage",
                        Severity = "Critical",
                        Endpoint = endpoint.Path,
                        Payload = "",
                        Evidence = "Sensitive data (credit card/email) found in response.",
                        DetectedAt = DateTime.UtcNow
                    });
                }
            }
            return vulns;
        }
    }
}