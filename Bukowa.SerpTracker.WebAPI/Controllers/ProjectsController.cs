using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Bukowa.SerpTracker.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ProjectsController : ControllerBase
{
    readonly ILogger<ProjectsController> _logger;
    readonly Database _database;

    public ProjectsController(ILogger<ProjectsController> logger, Database database)
    {
        _logger = logger;
        _database = database;
    }

    [HttpGet(Name = "GetProjects")]
    public IEnumerable<Project> Get()
    {
        return _database.Projects.FindAll().ToArray();
    }

    public class ProjectCreateBody : IValidatableObject
    {
        [Required] public string Name { get; set; }

        [Required] public string[] Queries { get; set; }

        [Required] public string[] Urls { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var db = validationContext.GetService<Database>();
            if (db.Projects.Exists(p => p.Name == Name))
            {
                yield return new ValidationResult("Project already exists", [nameof(Name)]);
            }
        }

        public Project AsProject()
        {
            return new Project
            {
                Name = Name,
                Queries = Queries,
                Urls = Urls
            };
        }
    }

    [HttpPost(Name = "CreateProject")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    public IActionResult Post([FromBody] ProjectCreateBody body)
    {
        if (_database.Locked(db =>
            {
                try
                {
                    return db.Projects.Insert(body.AsProject()) != null;
                }
                catch (LiteDB.LiteException exc)
                {
                    _logger?.LogError(exc, "An error occured while trying to insert the project");
                }

                return false;
            }))
        {
            return Created();
        }

        return BadRequest(ModelState);
    }
}