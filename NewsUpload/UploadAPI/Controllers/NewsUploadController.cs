using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using NewsFlowAPI.DTOs;
using NewsFlowAPI.Models;
using NewsFlowAPI.Services;
using StackExchange.Redis;

namespace UploadAPI.Controllers
{
    [ApiController]
    [Route("Uploader")]
    public class NewsUploadController : Controller
    {
        private readonly IBoltGraphClient _neo4j;
        private readonly IConnectionMultiplexer _redis;  
        private readonly IIdentifierService _ids;
        private readonly string _newestNewsKey;
        private readonly string _channelForNewestNews;


        public NewsUploadController(
            IConnectionMultiplexer redis,
            IBoltGraphClient neo4j,
            IIdentifierService ids)
        {
            this._redis = redis;
            this._neo4j = neo4j;
            _ids = ids;
            _newestNewsKey = "newestnews";
            _channelForNewestNews = "newest:channel";

        }
        [HttpPost("news/CreateNews2")]

        public async Task<ActionResult> CreateNews2([FromBody] NewsCreateDTO data)
        {
            try
            {

                //News node
                News news = new News
                {
                    Id = await _ids.NewsNext(),
                    Title = data.Title,
                    Summary = data.Summary,
                    Text = data.Text,
                    ImageUrl = data.ImageUrl,
                    AuthorId = data.authorId,
                    LocationId = data.locationId,
                    LikeCount = 0,
                    ViewsCount = 0
                };

                news.PostTime = DateTime.Now;



                //tags nodes
                /*   var tags = await _neo4j.Cypher
                       .Match("(t:Tag)")
                       .Where((Tag t) => data.tagsIds.Contains(t.Id))
                       .Return(t => t.As<Tag>())
                       .ResultsAsync;
                   var tagsList = tags.ToList();*/

                var tags = await _neo4j.Cypher
                    .Match("(t:Tag)")
                    .Where("any(tagId IN $tagsIds WHERE tagId = t.Id)")
                    .WithParam("tagsIds", data.tagsIds)
                    .Return(t => t.As<Tag>().Id)
                    .ResultsAsync;

                if (tags.Count() == 0)
                {
                    throw new Exception("THERE ISN'T ANY TAG");
                }

                var authorList = await _neo4j.Cypher
                   .Match("(u:User)")
                   .Where((User u) => u.Id == data.authorId && u.Role == "Author")
                   .Return(u => u.As<User>().Id)
                   .ResultsAsync;

                if (authorList.Count() == 0)
                {
                    throw new Exception("THERE ISN'T ANY AUTHOR WITH THAT ID");
                }


                await _neo4j.Cypher
                 .Create("(n:News $news)")
                 .WithParam("news", news)
                 .ExecuteWithoutResultsAsync();


                //tagovi
                await _neo4j.Cypher
                 .Match("(n:News), (t:Tag)")
                 .Where("any(tagId IN $tagsIds WHERE tagId = t.Id)")
                 .AndWhere((News n) => n.Id == news.Id)
                 .WithParam("tagsIds", data.tagsIds)
                 .Create("(n)-[:TAGGED]->(t)")
                 .ExecuteWithoutResultsAsync();

                await _neo4j.Cypher
                .Match("(n:News), (u:User)")
                .Where((News n, User u) => n.Id == news.Id && data.authorId == u.Id)
                .Create("(n)-[:WRITTEN]->(u)")
                .Create("(n)<-[:WRITTEN]-(u)")
                .ExecuteWithoutResultsAsync();

                await _neo4j.Cypher
                    .Match("(n:News), (l:Location)")
                    .Where((News n, Location l) => n.Id == news.Id && l.Id == data.locationId)
                    .Create("(n)-[:LOCATED]->(l)")
                    .Create("(n)<-[:LOCATED]-(l)")
                    .ExecuteWithoutResultsAsync();


                //insert in newest
                var db = _redis.GetDatabase();


                var numberOfReceived = await db.PublishAsync(_channelForNewestNews, news.Id);

                return Ok(news);
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }
    }
}
