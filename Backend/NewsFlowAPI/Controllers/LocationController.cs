using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using NewsFlowAPI.DTOs;
using NewsFlowAPI.Models;
using NewsFlowAPI.Services;
using StackExchange.Redis;

namespace NewsFlowAPI.Controllers
{
    [ApiController]
    [Route("location")]
    public class LocationController : Controller
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IBoltGraphClient _neo4j;
        private readonly IIdentifierService _ids;
        private readonly IQueryCacheService _cache;
        public LocationController(
            IConnectionMultiplexer redis,
            IBoltGraphClient neo4j,
            IIdentifierService ids,
            IQueryCacheService cache
            )
        {
            _redis = redis;
            _neo4j = neo4j;
            _ids = ids;
            _cache = cache;
        }
        //[Authorize]
        [HttpPost("create/{name}")]
        public async Task<ActionResult> CreateLocation(
            [FromRoute] string name)
        {
            var newLocation = new Location
            {
                Id = await _ids.LocationNext(),
                Name = name
            };


            await _neo4j.Cypher
                .Create("(l:Location $loc)")
                .WithParam("loc", newLocation)
                .ExecuteWithoutResultsAsync();

            return Ok("Location successfully added!");
        }
        //[Authorize]
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult> DeleteLocation([FromRoute] long id)
        {
            var loc = await _neo4j.Cypher
                .Match("(l:Location)")
                .Where((Location l) => l.Id == id)
                .Return(l => new
                {
                    l.As<Location>().Id,
                    l.As<Location>().Name
                })
                .Limit(1)
                .ResultsAsync;

            if (loc.Count() == 0)
            {
                return NotFound($"Location with Id:{id} not found");
            }

            await _neo4j.Cypher
                .Match("(l:Location)")
                .Where((Location l) => l.Id == id)
                .DetachDelete("l")
                .ExecuteWithoutResultsAsync();

            return Ok("Location deleted");
        }

        //[Authorize]
        [HttpPut("update/{id}")]
        public async Task<ActionResult> UpdateLocation([FromRoute] long id, [FromQuery] string name)
        {
            var loc= await _neo4j.Cypher
                .Match("(l:Location)")
                .Where((Location l) => l.Id == id)
                .Return(l => new
                {
                    l.As<Location>().Id,
                    l.As<Location>().Name
                })
                .Limit(1)
                .ResultsAsync;

            if (loc.Count() == 0)
            {
                return NotFound($"Location with Id:{id} not found");
            }
            await _neo4j.Cypher
                .Match("(l:Location)")
                .Where((Location l) => l.Id == id)
                .Set("l.Name=$name")
                .WithParam("name", name)
                .ExecuteWithoutResultsAsync();

            return Ok("Location updated");
        }

        //[Authorize]
        [HttpGet("get/{id}")]
        public async Task<ActionResult> GetLocation([FromRoute] long id)
        {
            var loc = await _neo4j.Cypher
                .Match("(l:Location)")
                .Where((Location l) => l.Id == id)
                .Return(l => new
                {
                    l.As<Location>().Id,
                    l.As<Location>().Name
                })
                .ResultsAsync;

            if (loc.Count() == 0)
            {
                return NotFound($"Location with Id:{id} not found");
            }
            return Ok(loc.ToList()[0].Name);
        }

        //[Authorize]
        [HttpGet("getByName/{name}")]
        public async Task<ActionResult> GetLocationByName([FromRoute] string name)
        {
            var loc = await _neo4j.Cypher
                .Match("(l:Location)")
                .Where((Location l) => l.Name == name)
                .Return(l => new
                {
                    l.As<Location>().Id,
                    l.As<Location>().Name
                })
                .ResultsAsync;

            if (loc.Count() == 0)
            {
                return NotFound($"Location with name: ${name} not found");
            }
            return Ok(loc.ToList()[0]);
        }


        //[Authorize]
        [HttpGet("get")]
        public async Task<ActionResult> GetAllLocations()
        {
            var loc = await _neo4j.Cypher
                .Match("(l:Location)")
                .Return(l => new
                {
                    l.As<Location>().Id,
                    l.As<Location>().Name
                })
                .ResultsAsync;
            return Ok(loc.ToList());
        }

        //[Authorize]
        [HttpGet("getLocationsByPrefix/{prefix}")]
        public async Task<ActionResult> GetAllLocations([FromRoute] string prefix)
        {
            var loc = await _neo4j.Cypher
                .Match("(l:Location)")
                .Where($"l.Name starts with '{prefix}'")
                .Return(l => new AuthorInfoDTO
                {
                    Id = l.As<Location>().Id,
                    Name = l.As<Location>().Name
                })
                .ResultsAsync;
            return Ok(loc.ToList());
        }

    }
}
