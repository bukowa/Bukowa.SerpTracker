using Microsoft.Extensions.Logging;

namespace Bukowa.SerpTracker;

public static class GoogleUrlBuilder
{
    const string Url =
        "https://www.google.{0}/search?complete=0&hl=en&q={1}&num={2}&start=0&filter=0&pws=0";

    public class Query
    {
        public required string query { get; set; }
        public string tld { get; set; } = "com";
        public int number { get; set; } = 100;

        public string Build() => string.Format(Url, tld, query, number);
    }
}

public class GoogleSearchConfig
{
    public Project Project { get; set; }
    public string Query { get; set; }
    public string Url { get; set; }
}

public interface IGoogleSearchExecutor
{
    Task<SearchResults?> SearchAsync(GoogleSearchConfig config);
}

/// <summary>
/// Implementation of <see cref="ISearchService"/>. 
/// </summary>
public class GoogleSearchService : ISearchService
{
    readonly ILogger? _logger;
    readonly IGoogleSearchExecutor _googleSearchExecutor;

    public GoogleSearchService(ILogger? logger, IGoogleSearchExecutor googleSearchExecutor)
    {
        _logger = logger;
        _googleSearchExecutor = googleSearchExecutor;
    }

    public async Task<SearchResults?> Search(Project project, string query)
    {
        try
        {
            var url = new GoogleUrlBuilder.Query { query = query }.Build();
            _logger?.LogInformation("Searching Project: {Project} for {Query}", project, query);
            return await _googleSearchExecutor.SearchAsync(new GoogleSearchConfig
            {
                Project = project,
                Query = query,
                Url = url
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Searching failed for {Project} and {Query}", project, query);
            return null;
        }
    }
}