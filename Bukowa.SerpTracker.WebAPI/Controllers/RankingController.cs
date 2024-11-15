using Microsoft.AspNetCore.Mvc;

namespace Bukowa.SerpTracker.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class RankingController : ControllerBase
{
    readonly ILogger<RankingController> _logger;
    readonly Database _database;

    public RankingController(ILogger<RankingController> logger, Database database)
    {
        _logger = logger;
        _database = database;
    }

    [HttpGet(Name = "GetRankings")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProjectNode>))]
    public IEnumerable<ProjectNode> Get()
    {
        var rankings = Ranking.CalculateNodes(_database.Projects.FindAll().ToList(), _database.SearchResults.FindAll().ToList());
        return rankings.AsEnumerable();
    }
    
}