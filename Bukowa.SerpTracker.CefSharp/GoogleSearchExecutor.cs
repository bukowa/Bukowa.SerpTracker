using CefSharp;
using CefSharp.OffScreen;
using Microsoft.Extensions.Logging;

namespace Bukowa.SerpTracker.CefSharp;

public interface IGoogleProxyService
{
    Proxy? GetProxy(GoogleSearchConfig config);
}

public class GoogleSearchExecutor : IGoogleSearchExecutor
{
    public string CefRootCachePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        @"Bukowa.SerpTracker\CefSharp\RootCache"
    );

    public string NewCefChildCachePath(string uuid) => Path.Combine(CefRootCachePath, uuid);

    IGoogleProxyService? _proxyService;
    ILogger? _logger;

    public GoogleSearchExecutor(ILogger? logger, IGoogleProxyService proxyService)
    {
        _logger = logger;
        _proxyService = proxyService;
    }

    public GoogleSearchExecutor(IGoogleProxyService proxyService)
    {
        _logger = null;
        _proxyService = proxyService;
    }

    public GoogleSearchExecutor()
    {
        _logger = null;
        _proxyService = null;
    }
    public void Initialize()
    {
        var settings = new CefSettings
        {
            RootCachePath = CefRootCachePath,
        };

        Cef.Initialize(settings);
    }

    public async Task<SearchResults?> SearchAsync(GoogleSearchConfig config)
    {
        var uuid = Guid.NewGuid().ToString();
        var cachePath = NewCefChildCachePath(uuid);
        _logger?.LogInformation("Starting with uuid: {uuid}", uuid);

        var rqBuilder = RequestContext.Configure();
        rqBuilder = rqBuilder.WithCachePath(cachePath);

        var proxy = _proxyService?.GetProxy(config);

        if (proxy != null)
        {
            _logger?.LogInformation("Using {proxy} for uuid: {uuid}", proxy, uuid);
            rqBuilder = rqBuilder.WithProxyServer(
                scheme: proxy.Scheme,
                host: proxy.Host,
                port: proxy.Port
            );
        }

        var browser = new ChromiumWebBrowser(requestContext: rqBuilder.Create());

        if (proxy != null && proxy.Username != string.Empty)
        {
            browser.RequestHandler = new ProxyRequestHandler(proxy.Username, proxy.Password);
        }

        SearchResults? searchResults = null;
        try
        {
            _logger?.LogInformation("Starting browser with url: {url}", config.Url);
            await browser.LoadUrlAsync(config.Url);
            if (browser.Address.Contains("/sorry/index?continue"))
            {
                throw new Exception("Recaptcha detected");
            }

            _logger?.LogInformation("Executing javascript...");
            var text = await browser.EvaluateScriptAsync(
                """
                (() => {
                        const linksWithAttributesAndBr = Array.from(document.querySelectorAll('a[jsname][href][data-ved]'))
                            .filter(link => 
                                link.firstElementChild && 
                                link.firstElementChild.tagName === 'BR' &&
                                link.offsetParent !== null
                            );
                
                        // Get href values from the filtered links
                        const hrefs = linksWithAttributesAndBr.map(link => link.href);
                        return hrefs; // Return the array of hrefs
                    })()
                """
            );
            if (!text.Success) throw new Exception($"Javascript returned an error: {text.Message}");
            var objs = (List<object>)text.Result;
            var urls = objs.Cast<string>();
            searchResults = new SearchResults
            {
                Date = DateTime.Now,
                Urls = urls.ToArray(),
                Query = config.Query,
            };
        }
        catch (Exception exc)
        {
            _logger?.LogError(exc, "An error occured while searching for Google Search results in uuid: {uuid}", uuid);
            var image = await browser.CaptureScreenshotAsync();
            await File.WriteAllBytesAsync($"./error_images/{uuid}.jpg", image);
        }
        return searchResults;
    }
}