namespace Bukowa.SerpTracker.UnitTest;

public class GoogleSearchService_UnitTest
{

    public class GoogleSearchExecutor: IGoogleSearchExecutor
    {
        public SearchResults? ReturnValue = null;
        
        public Task<SearchResults?> SearchAsync(GoogleSearchConfig config)
        {
            return Task.FromResult(ReturnValue);
        }
    }
    
    [Fact]
    public async Task TestGoogleSearchService()
    {

        var searchExecutor = new GoogleSearchExecutor();
        searchExecutor.ReturnValue = null;
        var searchService = new GoogleSearchService(null, searchExecutor);
        var result = await searchService.Search(new Project(), "Google");
        Assert.Null(result);
        searchExecutor.ReturnValue = new SearchResults();
        result = await searchService.Search(new Project(), "Google");
        Assert.NotNull(result);
    }
}