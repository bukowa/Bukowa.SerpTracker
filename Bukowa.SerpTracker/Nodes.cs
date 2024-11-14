namespace Bukowa.SerpTracker;


public class ProjectNode
{
    public List<QueryNode> Nodes { get; set; } = new();
    public Project Project { get; set; }
}

public class QueryNode
{
    public List<UrlNode> Nodes { get; set; } = new();
    
    public string Query { get; set; }
    public int TopPosition { get; set; }
    public DateTime? LastDate { get; set; }
    public Project Project { get; set; }
}

public class UrlNode
{
    public string Url { get; set; }
    public int Position { get; set; }
    public DateTime? Date { get; set; }
    public string Query { get; set; }
    public List<SearchResults> SearchResults { get; set; } = new();
}
