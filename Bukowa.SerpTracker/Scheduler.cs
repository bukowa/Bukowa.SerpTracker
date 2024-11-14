using Microsoft.Extensions.Logging;

namespace Bukowa.SerpTracker;

/// <summary>
/// Used for searching.
/// </summary>
public interface ISearchResultsEngine
{
    public SearchResults? Search(Project project, string query);
}

public interface IProjectsService
{
    public IEnumerable<Project> All();
}

public interface ISearchResultsService
{
    public IEnumerable<SearchResults> All();
    public bool Insert(SearchResults searchResult);
}

/// <summary>
/// Database implementation.
/// </summary>
/// <param name="database"></param>
public class ProjectsService(Database database) : IProjectsService
{
    public IEnumerable<Project> All() => database.Projects.FindAll();
}

/// <summary>
/// Database implementation.
/// </summary>
/// <param name="database"></param>
public class SearchResultsService(Database database) : ISearchResultsService
{
    public IEnumerable<SearchResults> All() => database.SearchResults.FindAll();
    public bool Insert(SearchResults searchResults) => database.Locked(db => db.SearchResults.Insert(searchResults));
}

public class SchedulerSettings
{
    public required TimeSpan WaitFor { get; set; } = TimeSpan.FromSeconds(10);
    public required TimeSpan Interval { get; set; } = TimeSpan.FromHours(24);
    public required ISearchResultsEngine SearchEngine { get; set; }
    public required IProjectsService ProjectsService { get; set; }
    public required ISearchResultsService SearchResultsService { get; set; }
}

public class Scheduler
{
    ILogger? _logger;

    public Scheduler(ILogger? logger)
    {
        _logger = logger;
    }

    public Scheduler()
    {
    }

    public async Task StartAsync(SchedulerSettings settings, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _logger?.LogInformation("Starting Scheduler");

            foreach (var project in settings.ProjectsService.All())
            {
                if (cancellationToken.IsCancellationRequested)
                    goto Finish;

                foreach (var projectQuery in project.Queries)
                {
                    if (cancellationToken.IsCancellationRequested)
                        goto Finish;

                    var searchResults = settings.SearchResultsService.All();

                    var lastSearchResult = searchResults
                        .Where(s => s.Query == projectQuery)
                        .OrderByDescending(s => s.Date)
                        .FirstOrDefault();

                    if (lastSearchResult == null || DateTime.UtcNow - lastSearchResult.Date >= settings.Interval)
                    {
                        try
                        {
                            var response = settings.SearchEngine.Search(project, projectQuery);

                            _logger?.LogInformation(
                                "Querying " +
                                "Project: {project} " +
                                "Query: {query} " +
                                "Last: {date}" +
                                "Response: {response}",
                                project.Name,
                                projectQuery,
                                lastSearchResult?.Date,
                                response
                            );

                            if (response != null)
                            {
                                settings.SearchResultsService.Insert(response);
                            }
                        }

                        catch (Exception ex)
                        {
                            _logger?.LogError(
                                ex,
                                "Querying " +
                                "Project: {project} " +
                                "Query: {query} " +
                                "Last: {date}",
                                project.Name,
                                projectQuery,
                                lastSearchResult?.Date);
                        }
                    }
                }
            }

            try
            {
                await Task.Delay(settings.WaitFor, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                _logger?.LogInformation("Scheduler is canceled");
                goto Finish;
            }
        }
        Finish:
        _logger?.LogInformation("Scheduler finished");
        return;
    }
}