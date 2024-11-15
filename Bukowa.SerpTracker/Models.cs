using System.Text.Json.Serialization;
using LiteDB;

namespace Bukowa.SerpTracker;

public class SearchResults
{
    [JsonIgnore] [BsonId] public ObjectId SearchResultsId { get; set; }
    public DateTime Date { get; set; }
    public string Query { get; set; }
    public string[] Urls { get; set; } = [];
    public byte[] Screenshot { get; set; } = [];
}

public class Project
{
    [JsonIgnore] [BsonId] public ObjectId ProjectId { get; set; }

    public string Name { get; set; }
    public string[] Urls { get; set; } = [];
    public string[] Queries { get; set; } = [];
    public string Language { get; set; } = "en";
    public string Tld { get; set; } = "com";
}