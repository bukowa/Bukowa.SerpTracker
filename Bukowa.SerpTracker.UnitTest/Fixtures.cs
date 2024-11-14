namespace Bukowa.SerpTracker.UnitTest;

public class DatabaseFixture : IDisposable
{
    
    public Database Database { get; }
    string path = Guid.NewGuid().ToString();
    
    public DatabaseFixture()
    {
        Database = new Database(path);
    }
    
    public void Dispose()
    {
        Database.Dispose();
        File.Delete(path);
    }
}