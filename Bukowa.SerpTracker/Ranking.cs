namespace Bukowa.SerpTracker;

public static class Ranking
{
    public static List<ProjectNode> CalculateNodes(List<Project> projects, List<SearchResults> searchResults)
    {
        var nodes = new List<ProjectNode>();

        {
            // PROJECTS
            projects.ForEach(project =>
            {
                // NODE PROJECT
                ProjectNode projectNode = new ProjectNode()
                {
                    Project = project,
                };

                // QUERIES
                foreach (var query in project.Queries)
                {
                    var topPosition = -1;
                    DateTime? lastDate = null;

                    // SEARCHES
                    var querySearchesDescending = searchResults
                        .Where(s => string.Equals(s.Query, query, StringComparison.CurrentCultureIgnoreCase))
                        .OrderByDescending(s=>s.Date).ToList();

                    // NODE QUERY
                    QueryNode queryNode = new QueryNode()
                    {
                        Query = query,
                    };

                    foreach (var url in project.Urls)
                    {
                        // NODE URL
                        UrlNode urlNode = new UrlNode()
                        {
                            Url = url,
                        };

                        // POSITION
                        var pos = 0;
                        DateTime? date = null;
                        
                        try
                        {
                            pos = querySearchesDescending[0].Urls.ToList().IndexOf(url) + 1;
                            date = querySearchesDescending[0].Date;
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            //
                        }
                        topPosition = pos > topPosition ? pos : topPosition;
                        lastDate = date > lastDate ? date : lastDate;

                        // NODE URL
                        urlNode.Position = pos;
                        urlNode.Date = date;
                        urlNode.SearchResults = querySearchesDescending;
                        urlNode.Query = query;
                        queryNode.Nodes.Add(urlNode);
                    }

                    // NODE QUERY 
                    queryNode.TopPosition = topPosition;
                    queryNode.LastDate = lastDate;
                    queryNode.Project = project;
                    projectNode.Nodes.Add(queryNode);
                }
                
                nodes.Add(projectNode);
            });
        }
        return nodes;
    }
}
