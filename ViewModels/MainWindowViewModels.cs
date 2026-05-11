using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

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
    private string _responseStatus = "";

    [ObservableProperty]
    private string _responseTime = "";

    [ObservableProperty]
    private string _responseBody = "";

    [ObservableProperty]
    private string _responseHeaders = "";

    public ObservableCollection<string> HttpMethods { get; } = new() { "GET", "POST", "PUT", "DELETE", "PATCH" };

    public ObservableCollection<KeyValueItem> QueryParams { get; }
    public ObservableCollection<KeyValueItem> Headers { get; }

    public ObservableCollection<CollectionNode> Collections { get; }

    public IRelayCommand SendCommand { get; }
    public IRelayCommand AddQueryParamCommand { get; }
    public IRelayCommand AddHeaderCommand { get; }

    public MainWindowViewModel()
    {
        QueryParams = new ObservableCollection<KeyValueItem>();
        Headers = new ObservableCollection<KeyValueItem>();

        SendCommand = new AsyncRelayCommand(SendRequestAsync);
        AddQueryParamCommand = new RelayCommand(() => AddParam(QueryParams));
        AddHeaderCommand = new RelayCommand(() => AddParam(Headers));

        ParseInitialQueryParams();

        Collections = new ObservableCollection<CollectionNode>
        {
            new CollectionNode
            {
                Name = "Test Collection",
                Children = { new CollectionNode { Name = "GET Test GET" } }
            }
        };
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

    private async Task SendRequestAsync()
    {
        ResponseStatus = "Sending...";
        ResponseBody = "";
        ResponseHeaders = "";
        ResponseTime = "";

        try
        {
            var urlBuilder = new StringBuilder(RequestUrl);
            if (QueryParams.Any(p => !string.IsNullOrEmpty(p.Key)))
            {
                var parameters = QueryParams
                    .Where(p => !string.IsNullOrEmpty(p.Key))
                    .Select(p => $"{Uri.EscapeDataString(p.Key ?? string.Empty)}={Uri.EscapeDataString(p.Value ?? string.Empty)}");
                var queryString = string.Join("&", parameters);
                urlBuilder.Append(urlBuilder.ToString().Contains('?') ? "&" : "?");
                urlBuilder.Append(queryString);
            }
            var fullUrl = urlBuilder.ToString();

            var method = new HttpMethod(SelectedHttpMethod);
            var request = new HttpRequestMessage(method, fullUrl);

            foreach (var header in Headers.Where(h => !string.IsNullOrEmpty(h.Key)))
            {
                if (!string.IsNullOrWhiteSpace(header.Key))
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value ?? string.Empty);
            }

            if ((method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Patch) && !string.IsNullOrEmpty(RequestBody))
            {
                request.Content = new StringContent(RequestBody, Encoding.UTF8, "application/json");
            }

            var stopwatch = Stopwatch.StartNew();
            var response = await _httpClient.SendAsync(request);
            stopwatch.Stop();
            ResponseTime = $"{stopwatch.ElapsedMilliseconds} ms";

            ResponseStatus = $"{(int)response.StatusCode} {response.ReasonPhrase}";
            ResponseBody = await response.Content.ReadAsStringAsync();

            var headers = response.Headers.Concat(response.Content.Headers)
                .Select(h => $"{h.Key}: {string.Join(", ", h.Value)}");
            ResponseHeaders = string.Join(Environment.NewLine, headers);
        }
        catch (Exception ex)
        {
            ResponseStatus = "Error";
            ResponseBody = ex.Message;
        }
    }
}