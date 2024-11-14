using LiteDB;

namespace Bukowa.SerpTracker;

public class Database : IDisposable
{
    object _lock = new();
    public readonly LiteDatabase DB;

    public Database(string path="data.db")
    {
        DB = new LiteDatabase(path);
    }

    public void Dispose()
    {
        DB?.Dispose();
    }

    public ILiteCollection<Project> Projects => DB.GetCollection<Project>("projects");
    public ILiteCollection<SearchResults> SearchResults => DB.GetCollection<SearchResults>("searchresults");

    public bool Locked(Func<Database, bool> action)
    {
        lock (_lock)
        {
            return action(this);
        }
    }

}