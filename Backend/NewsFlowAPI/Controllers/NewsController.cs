using Microsoft.AspNetCore.Mvc;
using Neo4jClient.Cypher;
using Newtonsoft.Json.Linq;
using NewsFlowAPI.DTOs;
using NewsFlowAPI.Models;
using Neo4jClient;
using NewsFlowAPI.Services;
using StackExchange.Redis;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace NewsFlowAPI.Controllers
{
    public class NewsController : Controller
    {
        private readonly string _trendingNewsKey = "trendingnews";
        private readonly IConnectionMultiplexer _redis;
        private readonly IBoltGraphClient _neo4j;
        private readonly IIdentifierService _ids;
        private readonly IConfiguration _configuration;
        private readonly IRedisNewsSubscriber _subscriber;

        public NewsController(
            IConnectionMultiplexer redis,
            IBoltGraphClient neo4j,
            IIdentifierService ids,
            IConfiguration config,
            IRedisNewsSubscriber subscriber
            )
        {
            _redis = redis;
            _neo4j = neo4j;
            _ids = ids;
            _configuration = config;
            _subscriber = subscriber;
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

        //[Authorize]
        [HttpGet("ClickNews/{id}")]
        public async Task<ActionResult> ClickNewsId([FromRoute] long id)
        {
            var db = _redis.GetDatabase();
            var news = db.StringGet($"news:{id}").ToString();
            if (String.IsNullOrEmpty(news))
            {
                var newsNeo = await _neo4j.Cypher
                    .Match("(n:News)")
                    .Where((News n) => n.Id == id)
                    .Return(n => n.As<News>())
                    .Limit(1)
                    .ResultsAsync;

                if (newsNeo.Count() == 0)
                {
                    return NotFound("News Not Found");
                }

                var newsNeoObject = newsNeo.First();
                newsNeoObject.ViewsCount += 1;
                float falloff = float.Parse(_configuration.GetSection("ViewsLastPeriodFalloff").Value);
                newsNeoObject.ViewsLastPeriod = (int)Math.Round(falloff * newsNeoObject.ViewsLastPeriod,0);

                //na net pise da ne moze transaction ako imaju razlicit ttl :(
                db.StringSet($"news:{id}",
                    System.Text.Json.JsonSerializer.Serialize(newsNeoObject),
                    expiry:TimeSpan.FromHours(float.Parse(_configuration.GetSection("NewsInRedisPeriodHours").Value)));
                
                db.StringSet($"newsExpire:{id}","",
                    expiry: TimeSpan.FromHours(0.96*float.Parse(_configuration.GetSection("NewsInRedisPeriodHours").Value)));
                
                _neo4j.Cypher
                    .Match("(n:News)")
                    .Where((News n) => n.Id == id)
                    .Set("n.ViewsCount=$views")
                    .WithParam("views", newsNeoObject.ViewsCount + 1)
                    .ExecuteWithoutResultsAsync();

                _subscriber.Subscribe(_redis, _neo4j);
                _subscriber.AddKey($"newsExpire:{id}");

                return Ok(newsNeoObject);
            }

            News newsObject= JsonConvert.DeserializeObject<News>(news);
            
            newsObject.ViewsLastPeriod += 1;
            newsObject.ViewsCount += 1;
            var updatedValue = JsonConvert.SerializeObject(newsObject);
            db.StringSet($"news:{id}", updatedValue, expiry: db.KeyTimeToLive($"news:{id}"));


            _neo4j.Cypher
                .Match("(n:News)")
                .Where((News n) => n.Id == id)
                .Set("n.ViewsCount=$views")
                .WithParam("views", newsObject.ViewsCount)
                .ExecuteWithoutResultsAsync();

            return Ok(newsObject);
        }

        //[Authorize]
        [HttpGet("GetTrending")]
        public async Task<ActionResult> GetTrending()
        {


            return BadRequest("Not implemented");
        }

    }
}
