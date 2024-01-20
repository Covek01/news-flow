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

        [Authorize]
        [HttpPost("create/{name}")]
        public async Task<ActionResult> CreateTag([FromRoute] string name)
        {
            try
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
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }


        [Authorize]
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult> DeleteTag([FromRoute] long id)
        {
            try
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
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }


        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<ActionResult> UpdateTag([FromRoute] long id, [FromQuery] string name)
        {
            try
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
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

            [Authorize]
        [HttpGet("get/{id}")]
        public async Task<ActionResult> GetTag([FromRoute] long id)
        {
            try
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
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpGet("getAllTags")]
        public async Task<ActionResult> GetAllTags()
        {
            try
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
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpGet("getByName/{name}")]
        public async Task<ActionResult> GetTagByName([FromRoute] string name)
        {
            try
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
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpGet("GetTags/{id}")]
        public async Task<ActionResult> GetTagsForUser([FromRoute] int id)
        {
            try
            {
                var tags = await _neo4j.Cypher
                    .Match("(t:Tag)<-[:FOLLOWS_TAG]-(u:User)")
                    .Where((User u) => u.Id == id)
                    .Return(t => t.As<Tag>())
                    .ResultsAsync;
                return Ok(tags);

            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
}

        [Authorize]
        [HttpGet("GetMyTags")]
        public async Task<ActionResult> GetMyTags()
        {
            try
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
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpPut("LikeTagFromNews/{newsId}")]
        public async Task<ActionResult> LikeTagsFromNews([FromRoute] long newsId)
        {
            try
            {
                var claims = HttpContext.User.Claims;

                var userId = Int32.Parse(claims.Where(c => c.Type == "Id").FirstOrDefault()?.Value ?? "-1");
                //userId = 6;

                if (userId == -1)
                    return Unauthorized("Error user not signed in");

                var tagsFromNews = await _neo4j.Cypher
                    .Match("(n:News)-[tg:TAGGED]->(t:Tag)")
                    .Where((News n) => n.Id == newsId)
                    .Return(t => t.As<Tag>().Id)
                    .ResultsAsync;

                var tagsFromUser = await _neo4j.Cypher
                    .Match("(u:User)-[ft:FOLLOWS_TAG]->(t:Tag)")
                    .Where((User u) => u.Id == userId)
                    .Return((ft, t) => new TagInterestDTO()
                    {
                        TagId = t.As<Tag>().Id,
                        LikeCount = ft.As<FollowsTag>().LikeCount,
                        InterestCoefficient = ft.As<FollowsTag>().InterestCoefficient
                    })
                    .ResultsAsync;

                var tagsFollowed = tagsFromUser.Select(t => t.TagId);

                var tagsFollowedAndLiked = tagsFromUser.IntersectBy(tagsFromNews, (t => t.TagId)).ToList();
                var tagsLikedNotFollowed = tagsFromNews.Except(tagsFollowedAndLiked.Select(t => t.TagId)).ToList();
                List<TagInterestDTO> tagsFollowedNotLiked = tagsFromUser.Except(tagsFollowedAndLiked).ToList();

                int numLikedAndFollowed = tagsFollowedAndLiked.Count();
                int numLikedNotFollowed = tagsLikedNotFollowed.Count();
                int numFollowedNotLiked = tagsFollowedNotLiked.Count();


                double maxInterest = double.Parse(_configuration.GetSection("MaxInterest").Value);
                //maxInterest/2.0 delilac se bira po izboru, sto je veci to se sporije menjaju praceni tagovi korisnika
                double interestQuant = (maxInterest / 2.0) / (numLikedAndFollowed + numLikedNotFollowed + numFollowedNotLiked + numLikedAndFollowed);



                List<TagInterestDTO> tagsLikedNotFollowedObjects = new List<TagInterestDTO>();
                foreach (var id in tagsLikedNotFollowed)
                {
                    tagsLikedNotFollowedObjects.Add(new TagInterestDTO()
                    {
                        TagId = id,
                        InterestCoefficient = interestQuant,
                        LikeCount = 1
                    });
                }

                foreach (var tag in tagsFollowedAndLiked)
                {
                    tag.InterestCoefficient += interestQuant;
                }
                foreach (var tag in tagsFollowedNotLiked)
                {
                    tag.InterestCoefficient -= interestQuant * (numLikedNotFollowed + numLikedAndFollowed) / numFollowedNotLiked;
                }

                //ako treba izbaciti elemente vrsi se normalizacija
                foreach (var tag in tagsFollowedNotLiked)
                {
                    if (tag.InterestCoefficient < 0)
                    {
                        await _neo4j.Cypher
                            .Match("(u:User)-[ft:FOLLOWS_TAG]->(t:Tag)")
                            .Where((User u) => u.Id == userId)
                            .AndWhere((Tag t) => t.Id == tag.TagId)
                            .Delete("ft")
                            .ExecuteWithoutResultsAsync();

                    }
                }
                tagsFollowedNotLiked.RemoveAll(tag => tag.InterestCoefficient < 0);
                double sum = tagsFollowedNotLiked.Sum(t => t.InterestCoefficient) + tagsLikedNotFollowedObjects.Sum(t => t.InterestCoefficient) + tagsFollowedAndLiked.Sum(t => t.InterestCoefficient);
                if (sum != maxInterest)
                {
                    numFollowedNotLiked = tagsFollowedNotLiked.Count;

                    foreach (var tag in tagsFollowedNotLiked)
                    {
                        tag.InterestCoefficient = tag.InterestCoefficient * maxInterest / sum;
                    }
                    foreach (var tag in tagsLikedNotFollowedObjects)
                    {
                        tag.InterestCoefficient = tag.InterestCoefficient * maxInterest / sum;
                    }
                    foreach (var tag in tagsFollowedAndLiked)
                    {
                        tag.InterestCoefficient = tag.InterestCoefficient * maxInterest / sum;
                    }
                }

                var followedTags = tagsFollowedNotLiked.Concat(tagsFollowedAndLiked).ToList();

                foreach (var tag in followedTags)
                {
                    await _neo4j.Cypher
                        .Match("(u:User)-[ft:FOLLOWS_TAG]->(t:Tag)")
                        .Where((User u) => u.Id == userId)
                        .AndWhere((Tag t) => t.Id == tag.TagId)
                        .Set("ft.LikeCount=$LikeCountt, ft.InterestCoefficient=$Interest")
                        .WithParams(new { LikeCountt = tag.LikeCount, Interest = tag.InterestCoefficient })
                        .ExecuteWithoutResultsAsync();
                }

                foreach (var tag in tagsLikedNotFollowedObjects)
                {
                    await _neo4j.Cypher
                        .Match("(u:User)", "(t:Tag)")
                        .Where((User u) => u.Id == userId)
                        .AndWhere((Tag t) => t.Id == tag.TagId)
                        .Create("(u)-[ft:FOLLOWS_TAG]->(t)")
                        .Set("ft.LikeCount=$LikeCount, ft.InterestCoefficient=$InterestCoefficient")
                        .WithParams(tag)
                        .ExecuteWithoutResultsAsync();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpGet("getTagsByPrefix/{prefix}")]
        public async Task<ActionResult> GetAllTagsByPrefix([FromRoute] string prefix)
        {
            try
            {
                var loc = await _neo4j.Cypher
                    .Match("(t:Tag)")
                    .Where($"t.Name starts with '{prefix}'")
                    .Return(t => new
                    {
                        t.As<Tag>().Id,
                        t.As<Tag>().Name
                    })
                    .ResultsAsync;
                return Ok(loc.ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
