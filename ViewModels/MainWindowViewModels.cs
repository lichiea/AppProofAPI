using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Avalonia.Media;

namespace AppProofAPI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly HttpClient _httpClient = new HttpClient();

    [ObservableProperty]
    private string _selectedHttpMethod = "GET";

    [ObservableProperty]
    private string _requestUrl = "https://postman-echo.com/get?test_key1=test_value1&test_key2=test_value2";

    [ObservableProperty]
    private string _requestBody = "";

    [ObservableProperty]
    private bool _enableSslVerification = true;

    [ObservableProperty]
    private bool _followRedirects = true;

    [ObservableProperty]
    private bool _detailedErrors = false;

    [ObservableProperty]
    private int _testTimeout = 30;

    [ObservableProperty]
    private string _userAgent = "ProofAPI/1.0";

    [ObservableProperty]
    private string _verdict = "FAILED";

    [ObservableProperty]
    private IBrush _verdictColor = Brushes.Red;

    [ObservableProperty]
    private string _criticalIssuesCount = "3 Critical Issues";

    [ObservableProperty]
    private string _averageResponseTime = "142 ms";

    [ObservableProperty]
    private string _responseTimeTrend = "+12 ms vs last run";

    [ObservableProperty]
    private string _requestsPerSecond = "850";

    [ObservableProperty]
    private string _piiLeaksStatus = "Detected";

    [ObservableProperty]
    private IBrush _piiLeaksColor = Brushes.Orange;

    [ObservableProperty]
    private string _piiLeaksDetail = "In 2 endpoints";

    [ObservableProperty]
    private string _selectedProjectName = "E-Commerce API v1";

    [ObservableProperty]
    private string _lastRunInfo = "Last run: Today, 10:42 AM";

    public ObservableCollection<ProjectNode> Projects { get; } = new();
    public ObservableCollection<string> HttpMethods { get; } = new() { "GET", "POST", "PUT", "DELETE", "PATCH" };
    public ObservableCollection<KeyValueItem> QueryParams { get; }
    public ObservableCollection<KeyValueItem> Headers { get; }

    [ObservableProperty]
    private ObservableCollection<Vulnerability> _vulnerabilities = new();

    [ObservableProperty]
    private ObservableCollection<LoadMetric> _loadMetrics = new();

    public ObservableCollection<string> Tabs { get; } = new()
    {
        "Overview", "Security Scan", "Load Profile", "Settings", "Request"
    };

    [ObservableProperty]
    private string _currentTab = "Overview";

    public bool IsOverviewSelected => CurrentTab == "Overview";
    public bool IsSecuritySelected => CurrentTab == "Security Scan";
    public bool IsLoadSelected => CurrentTab == "Load Profile";
    public bool IsSettingsSelected => CurrentTab == "Settings";
    public bool IsRequestSelected => CurrentTab == "Request";

    public IRelayCommand RunTestsCommand { get; }
    public IRelayCommand AddQueryParamCommand { get; }
    public IRelayCommand AddHeaderCommand { get; }
    public IRelayCommand SaveSettingsCommand { get; }

    public MainWindowViewModel()
    {
        QueryParams = new ObservableCollection<KeyValueItem>();
        Headers = new ObservableCollection<KeyValueItem>();

        RunTestsCommand = new AsyncRelayCommand(RunTestsAsync);
        AddQueryParamCommand = new RelayCommand(() => AddParam(QueryParams));
        AddHeaderCommand = new RelayCommand(() => AddParam(Headers));
        SaveSettingsCommand = new RelayCommand(SaveSettings);

        ParseInitialQueryParams();
        InitializeProjects();
        InitializeMockData();
    }

    private void InitializeProjects()
    {
        Projects.Add(new ProjectNode
        {
            Name = "E-Commerce API v1",
            Children =
            {
                new ProjectNode { Name = "GET /users" },
                new ProjectNode { Name = "POST /login" },
                new ProjectNode { Name = "GET /profile" }
            }
        });
        Projects.Add(new ProjectNode
        {
            Name = "Banking Service",
            Children =
            {
                new ProjectNode { Name = "GET /accounts" },
                new ProjectNode { Name = "POST /transfer" }
            }
        });
        Projects.Add(new ProjectNode
        {
            Name = "User Auth Microservice",
            Children =
            {
                new ProjectNode { Name = "POST /auth" },
                new ProjectNode { Name = "GET /verify" }
            }
        });
    }

    private void InitializeMockData()
    {
        Vulnerabilities.Add(new Vulnerability
        {
            Type = "BOLA (IDOR)",
            Endpoint = "GET /users/{id}",
            Severity = "Critical",
            Evidence = "User A can access User B data",
            Status = "Open"
        });
        Vulnerabilities.Add(new Vulnerability
        {
            Type = "SQL Injection",
            Endpoint = "POST /login",
            Severity = "High",
            Evidence = "Error based SQLi detected",
            Status = "Open"
        });
        Vulnerabilities.Add(new Vulnerability
        {
            Type = "Missing HSTS",
            Endpoint = "Global Header",
            Severity = "Medium",
            Evidence = "Strict-Transport-Security missing",
            Status = "Open"
        });
        Vulnerabilities.Add(new Vulnerability
        {
            Type = "PII Exposure",
            Endpoint = "GET /profile",
            Severity = "High",
            Evidence = "SSN found in response body",
            Status = "Open"
        });
        Vulnerabilities.Add(new Vulnerability
        {
            Type = "XSS Reflected",
            Endpoint = "GET /search?q=...",
            Severity = "Medium",
            Evidence = "<script> tag reflected",
            Status = "Open"
        });

        LoadMetrics.Add(new LoadMetric
        {
            Metric = "Total Requests",
            Value = "12,450",
            Threshold = "-",
            Status = "OK"
        });
        LoadMetrics.Add(new LoadMetric
        {
            Metric = "Error Rate",
            Value = "0.4%",
            Threshold = "< 1%",
            Status = "OK"
        });
        LoadMetrics.Add(new LoadMetric
        {
            Metric = "P95 Latency",
            Value = "450 ms",
            Threshold = "< 500 ms",
            Status = "OK"
        });
        LoadMetrics.Add(new LoadMetric
        {
            Metric = "Failed Transactions",
            Value = "12",
            Threshold = "0",
            Status = "Fail"
        });
    }

    private void ParseInitialQueryParams()
    {
        try
        {
            var uri = new Uri(RequestUrl);
            var query = uri.Query.TrimStart('?');
            if (!string.IsNullOrEmpty(query))
            {
                var pairs = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
                foreach (var pair in pairs)
                {
                    var parts = pair.Split('=');
                    if (parts.Length == 2)
                    {
                        var key = Uri.UnescapeDataString(parts[0]);
                        var value = Uri.UnescapeDataString(parts[1]);
                        AddParam(QueryParams, key, value);
                    }
                }
            }
            RequestUrl = uri.GetLeftPart(UriPartial.Path);
        }
        catch { }
    }

    private void AddParam(ObservableCollection<KeyValueItem> collection, string key = "", string value = "")
    {
        var item = new KeyValueItem(itemToRemove => collection.Remove(itemToRemove));
        item.Key = key;
        item.Value = value;
        collection.Add(item);
    }

    private async Task RunTestsAsync()
    {
        LastRunInfo = $"Last run: {DateTime.Now:HH:mm}";
        await Task.Delay(1500);

        Random rand = new Random();
        int newCriticals = rand.Next(1, 6);
        Verdict = newCriticals > 2 ? "FAILED" : "PASSED";
        VerdictColor = newCriticals > 2 ? Brushes.Red : Brushes.Green;
        CriticalIssuesCount = $"{newCriticals} Critical Issues";

        AverageResponseTime = $"{rand.Next(80, 250)} ms";
        ResponseTimeTrend = rand.Next(-20, 30) + " ms vs last run";
        RequestsPerSecond = $"{rand.Next(700, 1200)}";

        bool piiDetected = rand.Next(0, 2) == 1;
        PiiLeaksStatus = piiDetected ? "Detected" : "Clean";
        PiiLeaksColor = piiDetected ? Brushes.Orange : Brushes.Green;
        PiiLeaksDetail = piiDetected ? $"In {rand.Next(1, 4)} endpoints" : "No PII found";

        if (rand.Next(0, 3) == 0)
        {
            var newVuln = new Vulnerability
            {
                Type = "New Finding",
                Endpoint = RequestUrl,
                Severity = new[] { "Low", "Medium", "High", "Critical" }[rand.Next(4)],
                Evidence = "Simulated during test run",
                Status = "Open"
            };
            Vulnerabilities.Insert(0, newVuln);
        }

        var errorRateMetric = LoadMetrics.FirstOrDefault(m => m.Metric == "Error Rate");
        if (errorRateMetric != null)
        {
            double errorRate = rand.NextDouble() * 1.5;
            errorRateMetric.Value = $"{errorRate:F1}%";
            errorRateMetric.Status = errorRate < 1.0 ? "OK" : "Fail";
        }

        var p95Metric = LoadMetrics.FirstOrDefault(m => m.Metric == "P95 Latency");
        if (p95Metric != null)
        {
            int latency = rand.Next(200, 600);
            p95Metric.Value = $"{latency} ms";
            p95Metric.Status = latency < 500 ? "OK" : "Fail";
        }

        OnPropertyChanged(nameof(IsOverviewSelected));
        OnPropertyChanged(nameof(IsSecuritySelected));
        OnPropertyChanged(nameof(IsLoadSelected));
    }

    private void SaveSettings()
    {
        Debug.WriteLine($"Settings saved: SSL={EnableSslVerification}, Redirects={FollowRedirects}, Timeout={TestTimeout}s, UA={UserAgent}");
    }

    partial void OnCurrentTabChanged(string value)
    {
        OnPropertyChanged(nameof(IsOverviewSelected));
        OnPropertyChanged(nameof(IsSecuritySelected));
        OnPropertyChanged(nameof(IsLoadSelected));
        OnPropertyChanged(nameof(IsSettingsSelected));
        OnPropertyChanged(nameof(IsRequestSelected));
    }
}

public class Vulnerability
{
    public string Type { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public string Severity { get; set; } = "";
    public string Evidence { get; set; } = "";
    public string Status { get; set; } = "";
}

public class LoadMetric
{
    public string Metric { get; set; } = "";
    public string Value { get; set; } = "";
    public string Threshold { get; set; } = "";
    public string Status { get; set; } = "";
}

public class TabToBrushConverter : Avalonia.Data.Converters.IValueConverter
{
    public static TabToBrushConverter Instance { get; } = new TabToBrushConverter();

    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is string currentTab && parameter is string tabName)
            return currentTab == tabName ? (object)Brushes.DarkBlue : Brushes.Gray;
        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}