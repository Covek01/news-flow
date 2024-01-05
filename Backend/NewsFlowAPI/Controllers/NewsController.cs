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
        public async Task<ActionResult> CreateNews([FromBody] NewsDTO data)
        {
            News news = new
            {
                news.Title = data.Title,
                news.Summary = data.Summary,

            }
            using var session = _neo4j.Cypher
        }
    }
}
