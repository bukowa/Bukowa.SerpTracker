using System.Text.Json.Serialization;

namespace Bukowa.SerpTracker;


public class ProjectNode
{
    public Project Project { get; set; }
    public List<QueryNode> Nodes { get; set; } = new();
}

public class QueryNode
{
    public string Query { get; set; }
    public int TopPosition { get; set; }
    public DateTime? LastDate { get; set; }

    public List<UrlNode> Nodes { get; set; } = new();

    [JsonIgnore]
    public Project Project { get; set; }
}

public class UrlNode
{
    public string Url { get; set; }
    public int Position { get; set; }
    public DateTime? Date { get; set; }
    public string Query { get; set; }
    [JsonIgnore]
    public List<SearchResults> SearchResults { get; set; } = new();
}
