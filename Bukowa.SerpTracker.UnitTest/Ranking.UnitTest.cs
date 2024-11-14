namespace Bukowa.SerpTracker.UnitTest;

public class RankingUnitTest
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
        }
    };

    List<SearchResults> SearchResults = new()
    {
        new()
        {
            Query = "query1",
            Date = new DateTime(2000, 1, 1, 8, 0, 0),
            Urls = ["url1"],
        },
        new()
        {
            Query = "query2",
            Date = new DateTime(2001, 1, 1, 5, 0, 0),
            Urls = ["url1", "url2"],
        },
        new()
        {
            Query = "query2",
            Date = new DateTime(2001, 1, 1, 9, 0, 0),
            Urls = ["url2"],
        },
        new()
        {
            Query = "query3",
            Date = new DateTime(2002, 1, 2, 10, 0, 0),
            Urls = ["url1", "url2", "url3"],
        },
        new()
        {
            Query = "query3",
            Date = new DateTime(2002, 1, 2, 15, 0, 0),
            Urls = ["url1", "url3"],
        }
    };

    List<Project> prs => Projects;
    List<SearchResults> srs => SearchResults;

    /// <summary>
    /// Make sure that nodes are populated correctly.
    /// </summary>
    [Fact]
    public void TestGetNodes_Nodes()
    {
        var nodes = Ranking.CalculateNodes(Projects, SearchResults);
        Assert.Equal(2, nodes.Count);

        Assert.Equal("Project nr1", nodes[0].Project.Name);
        Assert.Equal("Project nr2", nodes[1].Project.Name);

        Assert.Equal(["query1", "query2", "query4"], nodes[0].Project.Queries);
        Assert.Equal(["query1", "query2", "query3", "query5"], nodes[1].Project.Queries);

        Assert.Equal(["url1", "url2", "url555"], nodes[0].Project.Urls);
        Assert.Equal(["url3"], nodes[1].Project.Urls);

        Assert.Equal(3, nodes[0].Nodes.Count);
        Assert.Equal(4, nodes[1].Nodes.Count);

        Assert.Equal(nodes[0].Project, nodes[0].Nodes[0].Project);
        Assert.Equal(nodes[1].Project, nodes[1].Nodes[0].Project);

        Assert.Equal(nodes[0].Project.Queries[0], nodes[0].Nodes[0].Query);
        Assert.Equal(nodes[1].Project.Queries[0], nodes[1].Nodes[0].Query);

        for (var i = 0; i < nodes[0].Nodes.Count; i++)
        {
            Assert.Equal(Projects[0].Queries[i], nodes[0].Nodes[i].Query);
            Assert.Equal(Projects[0].Urls, nodes[0].Nodes[i].Nodes.Select(n => n.Url));
        }

        for (var i = 0; i < nodes[1].Nodes.Count; i++)
        {
            Assert.Equal(Projects[1].Queries[i], nodes[1].Nodes[i].Query);
            Assert.Equal(Projects[1].Urls, nodes[1].Nodes[i].Nodes.Select(n => n.Url));
        }
    }

    /// <summary>
    /// Make sure are calculations are correct.
    /// </summary>
    [Fact]
    public void TestGetProject_Calculations()
    {
        var nodes = Ranking.CalculateNodes(Projects, SearchResults);
        
        // project 0 query 0 urls
        Assert.Equal(
            [srs[0]],
            nodes[0].Nodes[0].Nodes[0].SearchResults
        );
        Assert.Equal(
            [srs[0]],
            nodes[0].Nodes[0].Nodes[1].SearchResults
        );
        Assert.Equal(
            [srs[0]],
            nodes[0].Nodes[0].Nodes[2].SearchResults
        );
        
        // project 0  query 1 urls
        Assert.Equal(
            [srs[2], srs[1]],
            nodes[0].Nodes[1].Nodes[0].SearchResults
        );
        Assert.Equal(
            [srs[2], srs[1]],
            nodes[0].Nodes[1].Nodes[1].SearchResults
        );
        Assert.Equal(
            [srs[2], srs[1]],
            nodes[0].Nodes[1].Nodes[2].SearchResults
        );
        
        // project 0 query 2 urls
        Assert.Equal(
            [],
            nodes[0].Nodes[2].Nodes[0].SearchResults
        );
        Assert.Equal(
            [],
            nodes[0].Nodes[2].Nodes[1].SearchResults
        );
        Assert.Equal(
            [],
            nodes[0].Nodes[2].Nodes[2].SearchResults
        );
        
        // project 1 query 0 urls
        Assert.Equal(
            [srs[0]],
            nodes[1].Nodes[0].Nodes[0].SearchResults
        );
        Assert.Equal(
            [srs[0]],
            nodes[1].Nodes[0].Nodes[1].SearchResults
        );
        Assert.Equal(
            [srs[0]],
            nodes[1].Nodes[0].Nodes[2].SearchResults
        );
        
        // project 1 query 1 urls
        Assert.Equal(
            [srs[2], srs[1]],
            nodes[1].Nodes[1].Nodes[0].SearchResults
        );
        Assert.Equal(
            [srs[2], srs[1]],
            nodes[1].Nodes[1].Nodes[1].SearchResults
        );
        Assert.Equal(
            [srs[2], srs[1]],
            nodes[1].Nodes[1].Nodes[2].SearchResults
        );
        
        // project 1 query 2 urls
        Assert.Equal(
            [srs[4], srs[3]],
            nodes[1].Nodes[2].Nodes[0].SearchResults
        );
        Assert.Equal(
            [srs[4], srs[3]],
            nodes[1].Nodes[2].Nodes[1].SearchResults
        );
        Assert.Equal(
            [srs[4], srs[3]],
            nodes[1].Nodes[2].Nodes[2].SearchResults
        );

        // project 1 query 3 urls
        Assert.Equal(
            [],
            nodes[1].Nodes[3].Nodes[0].SearchResults
        );
        Assert.Equal(
            [],
            nodes[1].Nodes[3].Nodes[1].SearchResults
        );
        Assert.Equal(
            [],
            nodes[1].Nodes[3].Nodes[2].SearchResults
        );

    }
}