using Microsoft.Extensions.Logging;

namespace Bukowa.SerpTracker;

/// <summary>
/// Used for searching.
/// </summary>
public interface ISearchService
{
    public Task<SearchResults?> Search(Project project, string query);
}

/// <summary>
/// Used for gathering all projects.
/// </summary>
public interface IProjectsService
{
    public IEnumerable<Project> All();
}

/// <summary>
/// Used for gathering all search results.
/// </summary>
public interface ISearchResultsService
{
    public IEnumerable<SearchResults> All();
    public bool Insert(SearchResults searchResult);
}

/// <summary>
/// Database implementation for projects gathering.
/// </summary>
/// <param name="database"></param>
public class ProjectsService(Database database) : IProjectsService
{
    public IEnumerable<Project> All() => database.Projects.FindAll();
}

/// <summary>
/// Database implementation for search results gathering.
/// </summary>
/// <param name="database"></param>
public class SearchResultsService(Database database) : ISearchResultsService
{
    public IEnumerable<SearchResults> All() => database.SearchResults.FindAll();
    public bool Insert(SearchResults searchResults) => database.Locked(db => db.SearchResults.Insert(searchResults));
}

/// <summary>
/// Settings for configuring scheduler.
/// </summary>
public class SchedulerConfig
{
    public required TimeSpan WaitFor { get; set; } = TimeSpan.FromSeconds(10);
    public required TimeSpan Interval { get; set; } = TimeSpan.FromHours(24);
}

/// <summary>
/// Loop performing searching.
/// </summary>
public class SearchScheduler
{
    readonly ILogger? _logger;
    readonly ISearchService _searchEngine;
    readonly IProjectsService _projectsService;
    readonly ISearchResultsService _searchResultsService;
    readonly SchedulerConfig _config;

    public SearchScheduler(
        ILogger? logger,
        ISearchService searchEngine,
        IProjectsService projectsService,
        ISearchResultsService searchResultsService,
        SchedulerConfig config)
    {
        _logger = logger;
        _searchEngine = searchEngine;
        _projectsService = projectsService;
        _searchResultsService = searchResultsService;
        _config = config;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _logger?.LogInformation("Starting Scheduler");

            foreach (var project in _projectsService.All())
            {
                if (cancellationToken.IsCancellationRequested)
                    goto Finish;

                foreach (var projectQuery in project.Queries)
                {
                    if (cancellationToken.IsCancellationRequested)
                        goto Finish;

                    var searchResults = _searchResultsService.All();

                    var lastSearchResult = searchResults
                        .Where(s => s.Query == projectQuery)
                        .OrderByDescending(s => s.Date)
                        .FirstOrDefault();

                    if (lastSearchResult == null || DateTime.UtcNow - lastSearchResult.Date >= _config.Interval)
                    {
                        try
                        {
                            var response = await _searchEngine.Search(project, projectQuery);

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
                                _searchResultsService.Insert(response);
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
                await Task.Delay(_config.WaitFor, cancellationToken);
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