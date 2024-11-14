namespace Bukowa.SerpTracker.CefSharp.UnitTest;

public class GoogleSearchExecutorUnitTest
{
    [Fact]
    public async Task TestGoogleSearchExecutor_SearchAsync()
    {
        var searchExecutor = new GoogleSearchExecutor();
        searchExecutor.Initialize();
        var config = new GoogleSearchConfig
        {
            Project = new Project(),
            Query = "example.com",
            Url = new GoogleUrlBuilder.Query { query = "example.com" }.Build()
        };
        var results = await searchExecutor.SearchAsync(config);
        Assert.NotNull(results);
        Assert.Equal("https://example.com/", results.Urls[0]);
        Assert.True(results.Urls.Length > 8);
    }
}