using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using NewsFlowAPI.DTOs;
using NewsFlowAPI.Models;
using NewsFlowAPI.Services;
using StackExchange.Redis;
using System.Text.Json;

namespace NewsFlowAPI.Controllers
{
    [ApiController]
    [Route("tag")]
    public class TagController : Controller
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IBoltGraphClient _neo4j;
        private readonly IIdentifierService _ids;
        private readonly IConfiguration _configuration;

        public TagController(
            IConnectionMultiplexer redis,
            IBoltGraphClient neo4j,
            IIdentifierService ids,
            IConfiguration configuration
            )
        {
            _redis = redis;
            _neo4j = neo4j;
            _ids = ids;
            _configuration = configuration;
        }

        //[Authorize]
        [HttpPost("create/{name}")]
        public async Task<ActionResult> CreateTag([FromRoute] string name)
        {
            var newTag = new Tag
            {
                Id = await _ids.TagNext(),
                Name = name
            };

            await _neo4j.Cypher
                .Create("(t:Tag $tag)")
                .WithParam("tag", newTag)
                .ExecuteWithoutResultsAsync();

            var db = _redis.GetDatabase();
            db.SetAdd("tags:nodes", JsonSerializer.Serialize(new { Id = newTag.Id, Name = newTag.Name }));


            return Ok("Tag successfully added!");
        }
        //[Authorize]
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult> DeleteTag([FromRoute] long id)
        {
            var tag = await _neo4j.Cypher
                .Match("(t:Tag)")
                .Where((Tag t) => t.Id == id)
                .Return(t => new
                {
                    t.As<Tag>().Id,
                    t.As<Tag>().Name
                })
                .Limit(1)
                .ResultsAsync;

            if (tag.Count() == 0)
            {
                return NotFound($"Tag with Id:{id} not found");
            }
            await _neo4j.Cypher
                .Match("(t:Tag)")
                .Where((Tag t) => t.Id == id)
                .DetachDelete("t")
                .ExecuteWithoutResultsAsync();


            Tag tempTag = new Tag();
            tempTag.Id = id;
            tempTag.Name = tag.ToList().First().Name;

            var db = _redis.GetDatabase();
            await db.SetRemoveAsync("tags:nodes", JsonSerializer.Serialize(tempTag));

            return Ok("Tag deleted");

        }
        //[Authorize]
        [HttpPut("update/{id}")]
        public async Task<ActionResult> UpdateTag([FromRoute] long id, [FromQuery] string name)
        {
            var tag = await _neo4j.Cypher
                .Match("(t:Tag)")
                .Where((Tag t) => t.Id == id)
                .Return(t => new
                {
                    t.As<Tag>().Id,
                    t.As<Tag>().Name
                })
                .Limit(1)
                .ResultsAsync;

            if (tag.Count() == 0)
            {
                return NotFound($"Tag with Id:{id} not found");
            }
            await _neo4j.Cypher
                .Match("(t:Tag)")
                .Where((Tag t) => t.Id == id)
                .Set("t.Name=$name")
                .WithParam("name", name)
                .ExecuteWithoutResultsAsync();

            Tag tempTag = new Tag();
            tempTag.Id = id;
            tempTag.Name = tag.ToList().First().Name;

            var db = _redis.GetDatabase();
            await db.SetRemoveAsync("tags:nodes", JsonSerializer.Serialize(tempTag));
            tempTag.Name = name;
            await db.SetAddAsync("tags:nodes", JsonSerializer.Serialize(tempTag));

            return Ok("Tag updated");
        }

        //[Authorize(Roles ="writer")]
        [HttpGet("get/{id}")]
        public async Task<ActionResult> GetTag([FromRoute] long id)
        {
            var tag = await _neo4j.Cypher
               .Match("(t:Tag)")
               .Where((Tag t) => t.Id == id)
               .Return(t => new
               {
                   t.As<Tag>().Id,
                   t.As<Tag>().Name
               })
               .ResultsAsync;

            if (tag.Count() == 0)
            {
                return NotFound($"Tag with Id:{id} not found");
            }
            return Ok(tag.ToList()[0].Name);
        }

        //[Authorize(Roles ="writer")]
        [HttpGet("getAllTags")]
        public async Task<ActionResult> GetAllTags()
        {
            var tag = await _neo4j.Cypher
               .Match("(t:Tag)")
               .Return(t => new
               {
                   t.As<Tag>().Id,
                   t.As<Tag>().Name
               })
               .ResultsAsync;

            return Ok(tag);
        }

        [HttpGet("getByName/{name}")]
        public async Task<ActionResult> GetTagByName([FromRoute] string name)
        {
            var db = _redis.GetDatabase();

            //var nest = db.SetScanAsync("tags:nodes", "*S*");
            //var nest =
            string setName = "tags:nodes";
            string pattern = "*Id*Name*" + name + "*"; // Specify the pattern to match
            int count = 10;
            long cursor = 0;
            RedisResult scanResult;
            List<Tag> tagsMatched = new List<Tag>();
            do
            {
                scanResult = db.Execute("SSCAN", setName, cursor, "MATCH", pattern, "COUNT", count);

                var innerResults = (RedisResult[])scanResult;
                cursor = (long)innerResults[0];

                var matchingValues = (RedisValue[])innerResults[1];
                foreach (var value in matchingValues)
                {
                    Tag tempTag = JsonSerializer.Deserialize<Tag>(value.ToString());
                    tagsMatched.Add(tempTag);
                }

            } while (cursor != 0);




            return Ok(tagsMatched);
        }

        //[Authorize]
        [HttpGet("GetTags/{id}")]
        public async Task<ActionResult> GetTagsForUser([FromRoute] int id)
        {
            var tags = await _neo4j.Cypher
                .Match("(t:Tag)<-[:FOLLOWS_TAG]-(u:User)")
                .Where((User u) => u.Id == id)
                .Return(t => t.As<Tag>())
                .ResultsAsync;
            return Ok(tags);
        }

        //[Authorize]
        [HttpGet("GetMyTags")]
        public async Task<ActionResult> GetMyTags()
        {
            var claims = HttpContext.User.Claims;

            var userId = Int32.Parse(claims.Where(c => c.Type == "Id").FirstOrDefault()?.Value ?? "-1");
            //userId = 1;

            if (userId == -1)
                return Unauthorized("Error user not signed in");

            var tags = await _neo4j.Cypher
                .Match("(t:Tag)<-[:FOLLOWS_TAG]-(u:User)")
                .Where((User u) => u.Id == userId)
                .Return(t => t.As<Tag>())
                .ResultsAsync;
            return Ok(tags);
        }

        //[Authorize]
        [HttpPut("LikeTagFronNews/{newsId}")]
        public async Task<ActionResult> LikeTagsFromNews([FromRoute] long newsId)
        {
            var claims = HttpContext.User.Claims;

            var userId = Int32.Parse(claims.Where(c => c.Type == "Id").FirstOrDefault()?.Value ?? "-1");
            userId = 1;

            if (userId == -1)
                return Unauthorized("Error user not signed in");

            var tagsFromNews = await _neo4j.Cypher
                .Match("(n:News)-[tg:TAGGED]->(t:Tag)")
                .Where((News n) => n.Id == newsId)
                .Return(t => t.As<Tag>().Id)
                .ResultsAsync;

            var tagsFromUser =await _neo4j.Cypher
                .Match("(u:User)-[ft:FOLLOWS_TAG]->(t:Tag)")
                .Where((User u) => u.Id == userId)
                .Return(ft => new TagInterestDTO()
                {
                    TagId=ft.As<FollowsTag>().Tag!.Id,
                    LikeCount= ft.As<FollowsTag>().LikeCount,
                    InterestCoefficient=ft.As<FollowsTag>().InterestCoefficient
                })
                .ResultsAsync;

            var tagsFollowed = tagsFromUser.Select(t => t.TagId);

            var tagsFollowedAndLiked=tagsFromUser.IntersectBy(tagsFromNews,(t=>t.TagId)).ToList();
            var tagsLikedNotFollowed = tagsFromNews.Except(tagsFollowedAndLiked.Select(t=>t.TagId)).ToList();
            List<TagInterestDTO> tagsFollowedNotLiked = tagsFromUser.Except(tagsFollowedAndLiked).ToList();

            int numLikedAndFollowed = tagsFollowedAndLiked.Count();
            int numLikedNotFollowed = tagsLikedNotFollowed.Count();
            int numFollowedNotLiked = tagsFollowedNotLiked.Count();
            

            double maxInterest = double.Parse(_configuration.GetSection("MaxInterest").Value);
                //maxInterest/2.0 delilac se bira po izboru, sto je veci to se sporije menjaju praceni tagovi korisnika
            double interestQuant = (maxInterest/2.0) / (numLikedAndFollowed + numLikedNotFollowed + numFollowedNotLiked+numLikedAndFollowed);

            return BadRequest("Not finished");

        }
    }
}
