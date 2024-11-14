namespace Bukowa.SerpTracker.UnitTest;

public class SchedulerUnitTest
{
    List<Project> Projects = new()
    {
        new()
        {
            Name = "Project nr1",
            Queries = ["query1", "query2", "query4"],
            Urls = ["url1", "url2", "url555"],
        },
        new()
        {
            Name = "Project nr2",
            Queries = ["query1", "query2", "query3", "query5"],
            Urls = ["url3", "url2", "url1"],
        },
        new()
        {
            Name = "invalid",
        }
    };

    List<SearchResults> SearchResults = new()
    {
        new()
        {
            Query = "query1",
            Date = new DateTime(2000, 1, 1, 8, 0, 0),
            Urls = ["x", "y", "url1", "z"],
        },
    };

    List<Project> prs => Projects;
    List<SearchResults> srs => SearchResults;

    class SearchEngine : ISearchResultsEngine
    {
        public SearchResults? Search(Project project, string query)
        {
            if (project.Name == "invalid")
                return null;

            return new SearchResults
            {
                Query = query,
                Date = DateTime.UtcNow,
                Urls = ["url1", "url2"],
            };
        }
    }

    /// <summary>
    /// Make sure that nodes are populated correctly.
    /// </summary>
    [Fact]
    public async Task TestScheduler()
    {
        using var database = new DatabaseFixture();
        database.Database.Projects.InsertBulk(prs);
        
        var searchService = new SearchResultsService(database.Database);
        var projectsService = new ProjectsService(database.Database);

        var schedulerSettings = new SchedulerSettings()
        {
            Interval = TimeSpan.FromSeconds(1),
            ProjectsService = projectsService,
            SearchResultsService = searchService,
            SearchEngine = new SearchEngine(),
            WaitFor = TimeSpan.FromMilliseconds(10),
        };

        var scheduler = new Scheduler();
        var cncToken = new CancellationTokenSource();
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            await cncToken.CancelAsync();
        });
        
        await scheduler.StartAsync(schedulerSettings, cncToken.Token);
        Assert.Equal(5, searchService.All().Count());
    }
    
}