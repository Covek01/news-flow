using Microsoft.AspNetCore.Mvc;
using Neo4jClient.Cypher;
using Newtonsoft.Json.Linq;
using NewsFlowAPI.DTOs;
using NewsFlowAPI.Models;
using Neo4jClient;
using NewsFlowAPI.Services;
using StackExchange.Redis;

namespace NewsFlowAPI.Controllers
{
    public class NewsController : Controller
    {
        private readonly string _trendingNewsKey = "trendingnews";
        private readonly IConnectionMultiplexer _redis;
        private readonly IBoltGraphClient _neo4j;
        private readonly IIdentifierService _ids;

        public NewsController(
            IConnectionMultiplexer redis,
            IBoltGraphClient neo4j,
            IIdentifierService ids
            )
        {
            _redis = redis;
            _neo4j = neo4j;
            _ids = ids;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("createnews")]
        public async Task<ActionResult> CreateNews([FromBody] NewsCreateDTO data)
        {
            try
            {
                News news = new News
                {
                    Id = await _ids.NewsNext(),
                    Title = data.Title,
                    Summary = data.Summary,
                    Text = data.Text,
                    ImageUrl = data.ImageUrl
                };

                news.PostTime = DateTime.Now;

                await _neo4j.Cypher
                    .Create("(n:News $news)")
                    .WithParam("news", news)
                    .ExecuteWithoutResultsAsync();

                var location = await _neo4j.Cypher
                    .Match("(l:Location)")
                    .Where((Location l) => l.Id == data.locationId)
                    .Return(loc => loc.As<Location>())
                    .ResultsAsync;

                await _neo4j.Cypher
                    .Match("(n:News)")
                    .Where((News n) => n.Id == news.Id)
                    .Create("(n)-[:LOCATED]->(loc:Location $l)")
                    .WithParam("l", location)
                    .ExecuteWithoutResultsAsync();

                return Ok();
            }
            catch(Exception e)
            {
                return StatusCode(500);
            }
        }
    }
}
