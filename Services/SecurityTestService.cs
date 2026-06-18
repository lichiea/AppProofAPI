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
            ["SQLi"] = new() { "' OR '1'='1", "'; DROP TABLE users;--", "1 AND 1=1", "\" OR \"\"=\"" },
            ["XSS"] = new() { "<script>alert(1)</script>", "<img src=x onerror=alert(1)>", "\"><svg/onload=alert(1)>" },
            ["PathTraversal"] = new() { "../../../etc/passwd", "..\\..\\windows\\win.ini", "%2e%2e%2f%2e%2e%2fetc%2fpasswd" },
            ["SSTI"] = new() { "{{7*7}}", "${7*7}", "#{7*7}" },
            ["XXE"] = new() { "<?xml version=\"1.0\"?><!DOCTYPE root [<!ENTITY test SYSTEM 'file:///etc/passwd'>]><root>&test;</root>" }
        };

        private readonly List<string> _securityHeaders = new()
        {
            "Content-Security-Policy", "X-Content-Type-Options", "X-Frame-Options", "Strict-Transport-Security"
        };

        // Regex для поиска PII (кредитные карты, email)
        private readonly Regex _piiRegex = new(@"\b(?:\d{4}[- ]?){3}\d{4}\b|\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.Compiled);

        // Regex для детекции SQL-ошибок в ответе
        private readonly Regex _sqliErrorRegex = new(@"(?i)(sql syntax|mysql_fetch|ora-\d+|postgresql|syntax error|unclosed quotation)", RegexOptions.Compiled);

        public SecurityTestService(IAuthService authService)
        {
            var handler = new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) };
            _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) };
            _authService = authService;
        }

        public async Task<List<Vulnerability>> RunSecurityTestsAsync(ApiSpec spec)
        {
            var vulnerabilities = new List<Vulnerability>();

            System.Diagnostics.Debug.WriteLine($"[МТБ] Запуск тестирования безопасности. BaseUrl: {spec.BaseUrl}");
            System.Diagnostics.Debug.WriteLine($"[МТБ] Количество эндпоинтов: {spec.Endpoints.Count}");

            // 1. Пассивная проверка заголовков безопасности
            if (!string.IsNullOrWhiteSpace(spec.BaseUrl))
            {
                var headersVulns = await CheckSecurityHeadersAsync(spec.BaseUrl);
                vulnerabilities.AddRange(headersVulns);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[МТБ] BaseUrl пустой, пропускаем проверку заголовков");
            }

            // 2. Активный фаззинг по каждому эндпоинту и параметру
            foreach (var endpoint in spec.Endpoints)
            {
                foreach (var param in endpoint.Parameters)
                {
                    foreach (var attackType in _fuzzPayloads)
                    {
                        foreach (var payload in attackType.Value)
                        {
                            var vuln = await TestFuzzAsync(spec.BaseUrl, endpoint, param.Key, payload, attackType.Key);
                            if (vuln != null)
                            {
                                vulnerabilities.Add(vuln);
                            }
                        }
                    }
                }
            }

            // 3. Поиск утечек PII в ответах
            var piiVulns = await CheckPiiLeakageAsync(spec);
            vulnerabilities.AddRange(piiVulns);

            System.Diagnostics.Debug.WriteLine($"[МТБ] Тестирование завершено. Найдено уязвимостей: {vulnerabilities.Count}");
            return vulnerabilities;
        }

        private async Task<List<Vulnerability>> CheckSecurityHeadersAsync(string baseUrl)
        {
            var vulns = new List<Vulnerability>();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, baseUrl);
                await _authService.AddAuthHeaderAsync(request);
                var response = await _httpClient.SendAsync(request);

                foreach (var header in _securityHeaders)
                {
                    if (!response.Headers.Contains(header) && !response.Content.Headers.Contains(header))
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
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[МТБ] HttpRequestException при проверке заголовков: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[МТБ] Ошибка при проверке заголовков: {ex.Message}");
            }
            return vulns;
        }

        private async Task<Vulnerability?> TestFuzzAsync(string baseUrl, ApiEndpoint endpoint, string paramName, string payload, string attackType)
        {
            // Формируем абсолютный URL (исправляет InvalidOperationException)
            var cleanBaseUrl = baseUrl?.TrimEnd('/') ?? "";
            var cleanPath = endpoint.Path.TrimStart('/');
            var url = $"{cleanBaseUrl}/{cleanPath}?{paramName}={Uri.EscapeDataString(payload)}";

            System.Diagnostics.Debug.WriteLine($"[МТБ] Фаззинг ({attackType}): {url}");

            var request = new HttpRequestMessage(new HttpMethod(endpoint.Method), url);
            await _authService.AddAuthHeaderAsync(request);

            try
            {
                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                // Детекция SQLi: ищем ошибки СУБД в ответе
                if (attackType == "SQLi" && _sqliErrorRegex.IsMatch(body))
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
                // Детекция XSS: ищем неотфильтрованное отражение пейлоада
                else if (attackType == "XSS" && body.Contains(payload))
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
                // Детекция Path Traversal: ищем маркеры успешного чтения файлов
                else if (attackType == "PathTraversal" && (body.Contains("root:") || body.Contains("[extensions]")))
                {
                    return new Vulnerability
                    {
                        Type = attackType,
                        Severity = "High",
                        Endpoint = endpoint.Path,
                        Payload = payload,
                        Evidence = "Обнаружены признаки чтения системных файлов.",
                        DetectedAt = DateTime.UtcNow
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                // Перехватываем сетевые ошибки (требование NR-05)
                System.Diagnostics.Debug.WriteLine($"[МТБ] HttpRequestException при фаззинге: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                // Перехватываем ошибки формирования URL
                System.Diagnostics.Debug.WriteLine($"[МТБ] InvalidOperationException: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                // Таймаут запроса
                System.Diagnostics.Debug.WriteLine($"[МТБ] Таймаут при фаззинге: {url}");
            }
            catch (Exception ex)
            {
                // Любые другие ошибки
                System.Diagnostics.Debug.WriteLine($"[МТБ] Неизвестная ошибка: {ex.Message}");
            }

            return null;
        }

        private async Task<List<Vulnerability>> CheckPiiLeakageAsync(ApiSpec spec)
        {
            var vulnerabilities = new List<Vulnerability>();

            foreach (var endpoint in spec.Endpoints)
            {
                // Формируем абсолютный URL
                var cleanBaseUrl = spec.BaseUrl?.TrimEnd('/') ?? "";
                var cleanPath = endpoint.Path.TrimStart('/');
                var url = $"{cleanBaseUrl}/{cleanPath}";

                try
                {
                    var request = new HttpRequestMessage(new HttpMethod(endpoint.Method), url);
                    await _authService.AddAuthHeaderAsync(request);
                    var response = await _httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var body = await response.Content.ReadAsStringAsync();
                        if (_piiRegex.IsMatch(body))
                        {
                            vulnerabilities.Add(new Vulnerability
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
                }
                catch (HttpRequestException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[МТБ] HttpRequestException при проверке PII: {ex.Message}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[МТБ] Ошибка при проверке PII: {ex.Message}");
                }
            }
            return vulnerabilities;
        }
    }
}