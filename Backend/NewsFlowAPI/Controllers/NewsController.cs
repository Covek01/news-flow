using Microsoft.AspNetCore.Mvc;
using Neo4jClient.Cypher;
using Newtonsoft.Json.Linq;
using NewsFlowAPI.DTOs;
using NewsFlowAPI.Models;
using Neo4jClient;
using NewsFlowAPI.Services;
using StackExchange.Redis;

using Neo4j.Driver;
using System.Linq;
using Newtonsoft.Json;

using Neo4j.Driver;
using Neo4j.Driver;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration.UserSecrets;


namespace NewsFlowAPI.Controllers
{

    public class NewsController : Controller
    {
        private readonly string _newestNewsKey;
        private readonly int _maxLengthOfNewestNews;
        private readonly string _channelForNewestNews;
        private readonly IConnectionMultiplexer _redis;
        private readonly IBoltGraphClient _neo4j;
        private readonly IIdentifierService _ids;
        private readonly IConfiguration _configuration;
        private readonly IRedisNewsSubscriber _subscriber;
        private readonly IRedisNewsSubscriber _trendSub;
        private readonly IQueryCacheService _queryCache;

        public NewsController(
            IConnectionMultiplexer redis,
            IBoltGraphClient neo4j,
            IIdentifierService ids,
            IConfiguration config,
            IRedisNewsSubscriber subscriber,
            IRedisNewsSubscriber trendSub,
            IQueryCacheService queryCache
            )
        {
            _redis = redis;
            _neo4j = neo4j;
            _ids = ids;
            _newestNewsKey = "newestnews";
            _maxLengthOfNewestNews = 20;
            _subscriber = subscriber;
            _channelForNewestNews = "newest:channel";

            _configuration = config;
            _subscriber = subscriber;
            _trendSub = trendSub;
            _queryCache = queryCache;

        }

        /* public async Task CheckAndInitializeKeysInRedis()
         {
             var db = _redis.GetDatabase();

             if (!db.KeyExists(_trendingNewsKey))
             {
                 await db.ListLeftPushAsync(_trendingNewsKey, )
             }


         }*/

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("news/createnews")]
        public async Task<ActionResult> CreateNews([FromBody] NewsCreateDTO data)
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

                //if max length of new news is here, then take out the last one 
                if (db.ListLength(_newestNewsKey) > 20)
                {
                    db.ListRightPop(_newestNewsKey);
                }

                NewsRedisStorageDTO newsForRedis = new NewsRedisStorageDTO
                {
                    Id = news.Id,
                    Title = news.Title,
                    Summary = news.Summary,
                    Text = news.Text,
                    ImageUrl = news.ImageUrl,
                    authorId = news.AuthorId,
                    locationId = news.LocationId,
                    likeCount = news.LikeCount,
                    viewsCount = news.ViewsCount,
                    PostTime = news.PostTime
                };

                var newsForRedisJson = JsonConvert.SerializeObject(newsForRedis);

                db.ListLeftPush(_newestNewsKey, newsForRedisJson);

                await db.PublishAsync(_channelForNewestNews, newsForRedisJson);

                return Ok(news);
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpGet("news/getAllNews")]
        public async Task<ActionResult> GetAllNews()
        {
            try
            {
                var news = await _neo4j.Cypher
                    .Match("(n:News)")
                    .Return(n => n.As<News>())
                    .ResultsAsync;

                return Ok(news);
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpDelete("news/deleteNewsById/{id}")]
        public async Task<ActionResult> DeleteNewsById([FromRoute] int id)
        {
            try
            {

                var newsList = await _neo4j.Cypher
                    .Match("(n:News)")
                    .Where((News n) => n.Id == id)
                    .Return(n => n.As<News>())
                    .ResultsAsync;

                await _neo4j.Cypher
                    .Match("(n:News)")
                    .Where((News n) => n.Id == id)
                    .DetachDelete("n")
                    .ExecuteWithoutResultsAsync();

                var news = newsList.FirstOrDefault();
                NewsRedisStorageDTO newsForRedis = new NewsRedisStorageDTO
                {
                    Id = news.Id,
                    Title = news.Title,
                    Summary = news.Summary,
                    Text = news.Text,
                    ImageUrl = news.ImageUrl,
                    authorId = news.AuthorId,
                    locationId = news.LocationId,
                    PostTime = news.PostTime
                };

                var db = _redis.GetDatabase();
                var serialized = JsonConvert.SerializeObject(newsForRedis);
                //Deletes from newest if it's there
                long count = db.ListRemove(_newestNewsKey, serialized);

                return Ok(count);
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpDelete("news/deleteAllNews")]
        public async Task<ActionResult> DeleteAllNews()
        {
            try
            {
                await _neo4j.Cypher
                    .Match("(n:News)")
                    .DetachDelete("n")
                    .ExecuteWithoutResultsAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpGet("news/getNewsByAuthor/{authorName}")]
        public async Task<ActionResult> GetNewsByAuthor([FromRoute] string authorName)
        {
            try
            {
                var news = (await _neo4j.Cypher
                    .Match("(n:News)<-[:HAS_WRITTEN]-(u:User)")
                    .Where((User u) => u.Name == authorName)
                    .Return(n => n.As<News>())
                    .ResultsAsync)
                    .ToList();

                return Ok(news.ToList());
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpGet("news/getNewsById/{id}")]
        public async Task<ActionResult> GetNewsById([FromRoute] long id)
        {
            try
            {
                var p = (await _neo4j.Cypher
                    .Match("(n:News)")
                    .Where((News n) => n.Id == id)
                    .Return(n => n.As<News>())
                    .ResultsAsync)
                    .FirstOrDefault();


                var author = (await _neo4j.Cypher
                            .Match("(u:User)")
                            .Where((User u) => u.Id == p.AuthorId)
                            .Return(u => u.As<User>().Name)
                            .ResultsAsync).FirstOrDefault();
                    
                var tagsIds = (await _neo4j.Cypher
                    .Match("(n:News)-[:TAGGED]->(t:Tag)")
                    .Where((News n) => n.Id == id)
                    .Return(t => t.As<Tag>().Id)
                    .ResultsAsync)
                    .ToList();
                var newsReturnResult = new NewsReturnDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Text = p.Text,
                    Summary = p.Summary,
                    ImageUrl = p.ImageUrl,
                    locationId = p.LocationId,
                    ViewsCount = p.ViewsCount,
                    LikeCount = p.LikeCount,
                    authorId = p.AuthorId,
                    PostTime = p.PostTime,
                    authorName = author,
                    tagsIds = new List<long>(tagsIds)
                };


                return Ok(newsReturnResult);

            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }


        [HttpGet("news/getNewestNews")]
        public async Task<ActionResult> GetNewestNews()
        {
            try
            {
                var db = _redis.GetDatabase();

                var news = await db.ListRangeAsync(_newestNewsKey);
                var newsDeserialized = new List<News>();

                foreach (var n in news)
                {
                    newsDeserialized.Add(JsonConvert.DeserializeObject<News>(n));
                }

                var newsReturnTasks = newsDeserialized.Select(async p =>
                {

                    var author = (await _neo4j.Cypher
                                    .Match("(u:User)")
                                    .Where((User u) => u.Id == p.AuthorId)
                                    .Return(u => u.As<User>().Name)
                                    .ResultsAsync).FirstOrDefault();

                    return new NewsReturnDTO
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Text = p.Text,
                        Summary = p.Summary,
                        ImageUrl = p.ImageUrl,
                        locationId = p.LocationId,
                        ViewsCount = p.ViewsCount,
                        LikeCount = p.LikeCount,
                        authorId = p.AuthorId,
                        PostTime = p.PostTime,
                        authorName = author
                    };
                });

                var newsReturnResults = await Task.WhenAll(newsReturnTasks);

                return Ok(newsReturnResults.ToList());
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("news/isNewsLikedByUser/{userId}/{newsId}")]
        public async Task<ActionResult> IsNewsLikedByUser([FromRoute] long userId, [FromRoute] long newsId)
        {
            try
            {
                var listCons = (await _neo4j.Cypher
                                  .Match("(u:User)-[:LIKES]->(n:News)")
                                  .Where((User u, News n) => u.Id == userId && n.Id == newsId)
                                  .Return((u, n) => new
                                  {
                                      userId = u.As<User>().Id,
                                      newsId = n.As<News>().Id
                                  }).ResultsAsync).ToList();

                bool isLiked = listCons.Any();
                return Ok(isLiked);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }


        [HttpPost("news/geteNewsByTags")]
        public async Task<ActionResult> GetNewsByTags([FromBody] List<long> tagIds)
        {
            try
            {
                var news = await _neo4j.Cypher
                    .Match("(n:News)-[:TAGGED]->(t:Tag)")
                    .Where("any(tagId IN $tagsIds WHERE tagId = t.Id)")
                    .WithParam("tagsIds", tagIds)
                    .ReturnDistinct(n => n.As<News>())
                    .ResultsAsync;



                var newsReturn = news.Select(p =>
                new NewsReturnDTO
                {
                    Title = p.Title,
                    Text = p.Text,
                    Summary = p.Summary,
                    ImageUrl = p.ImageUrl,
                    authorId = p.AuthorId,
                    locationId = p.LocationId
                });

                return Ok(newsReturn.ToList());
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }

        }

        //[Authorize]
        [HttpGet("news/ClickNews2/{id}")]
        public async Task<ActionResult> ClickNewsId2([FromRoute] long id)
        {
            var db = _redis.GetDatabase();
            var query = _neo4j.Cypher
                    .Match("(n:News)")
                    .Where((News n) => n.Id == id)
                    .Return(n => n.As<News>())
                    .Limit(1);

            var news = (await _queryCache.QueryCache(query, $"news:{id}", TimeSpan.FromHours(2)));
            db.StringSet($"shadow:news:{id}","shadow",expiry:TimeSpan.FromHours(0.95 * 2));
            _subscriber.Subscribe(_redis, _neo4j);
            _subscriber.AddKey($"shadow:news:{id}");
            if (news.Count()==0)
            {
                return NotFound("News not found!");
            }
            news.First().ViewsCount += 1;
            //var newsSingle = news.First();
            //newsSingle.ViewsCount += 1;
            //_neo4j.Cypher
            //        .Match("(n:News)")
            //        .Where((News n) => n.Id == id)
            //        .Set("n.ViewsCount=$views")
            //        .WithParam("views", news.ViewsCount + 1)
            //        .ExecuteWithoutResultsAsync();

            db.StringSet($"news:{id}", JsonConvert.SerializeObject(news), expiry: db.KeyTimeToLive($"news:{id}"));

            return Ok(news);
        }

        //[Authorize]
        [HttpGet("news/GetTrending2")]
        public async Task<ActionResult> GetTrending2()
        {
            var db = _redis.GetDatabase();
            var trending = db.SortedSetRangeByRankAsync("trending:news:", 0, 9, Order.Descending).Result;
            List<RedisValue> newsRedisValue = trending.ToList();
            List<News> newsObjects = new List<News>();
            if(newsRedisValue.Count > 0)
            {
                foreach (var nId in newsRedisValue)
                {
                    //newsObjects.Add(JsonConvert.DeserializeObject<News>(n.ToString()));
                    var query = _neo4j.Cypher
                    .Match("(n:News)")
                    .Where((News n) => n.Id == nId)
                    .Return(n => n.As<News>())
                    .Limit(1);
                    var news = (await _queryCache.QueryCacheNoAdd(query, $"news:{nId}"));

                    if (news.Count() == 0)
                    {
                        return BadRequest("Try again later");
                    }
                    var newsSingle = news.First();
                    newsObjects.Add(newsSingle);

                }
            }
            var expired = db.StringGet("trending:news:update").ToString();
            if (string.IsNullOrEmpty(expired))
            {
                db.StringSet("trending:news:update", "trending_expiration", TimeSpan.FromHours(0.5));
                _trendSub.Subscribe(_redis, _neo4j);
                _trendSub.AddKey("trending:news:update");
            }
            //await db.SortedSetAddAsync("trending:news:",null);
            return Ok(newsObjects);

        }

        //NE KORISTI SE 
        //[Authorize]
        [HttpGet("news/GetTrending")]
        public async Task<ActionResult> GetTrending()
        {
            return BadRequest("DEPRECATED");
            var db = _redis.GetDatabase();

            var trending = db.SortedSetRangeByRankAsync("trending:news:", 0, 9, Order.Descending).Result;
            List<RedisValue> newsRedisValue = trending.ToList();

            List<News> newsObjects = new List<News>();



            if (trending.Count() < 10)
            {
                string pattern = "news:*";
                var keysList = new List<RedisKey>();
                var cursor = default(long);

                do
                {
                    var result = db.Execute("SCAN", cursor.ToString(), "MATCH", pattern, "COUNT", "20");
                    var innerResult = (RedisResult[])result;

                    cursor = long.Parse((string)innerResult[0]);
                    var keys = (string[])innerResult[1];

                    foreach (var key in keys)
                    {
                        keysList.Add(key);
                    }
                } while (cursor != 0);

                var transaction = db.CreateTransaction();
                var newsList = new List<News>();

                foreach (var key in keysList)
                {
                    var news = db.StringGet(key).ToString();
                    var newsObject = JsonConvert.DeserializeObject<News>(news);
                    newsList.Add(newsObject);
                    db.StringSet("trending:expire:", "not-expired", expiry: TimeSpan.FromHours(2));
                    transaction.SortedSetAddAsync("trending:news:", news, newsObject.ViewsLastPeriod + newsObject.LikeCount / 3);
                }

                await transaction.ExecuteAsync();
                trending = db.SortedSetRangeByRankAsync("trending:news:", 0, 9, Order.Descending).Result;
                newsRedisValue = trending.ToList();

            }
            if (trending.Count() != 0)
            {

                foreach (var n in newsRedisValue)
                {
                    newsObjects.Add(JsonConvert.DeserializeObject<News>(n.ToString()));
                }
            }


            var expiredCheck = db.StringGet("trending:expire:");

            if (string.IsNullOrEmpty(expiredCheck))
            {
                foreach (var tr in newsObjects)
                {
                    await db.SortedSetDecrementAsync("trending:expire:", JsonConvert.SerializeObject(tr), tr.ViewsLastPeriod);
                    await db.SortedSetRemoveRangeByScoreAsync("trending:news:", int.MinValue, 0);
                }
            }
            return Ok(newsObjects);


        }


        ////[Authorize]
        //[HttpGet("GetTrending")]
        //public async Task<ActionResult> GetTrending()
        //{
        //    var db = _redis.GetDatabase();
        //    var trending = db.StringGet("trending:news:");
        //    if (string.IsNullOrEmpty(trending))
        //    {
        //        string pattern = "news:*";
        //        List<string> keysList = new List<string>();
        //        var cursor = default(long);
        //        do
        //        {
        //            var result = db.Execute("SCAN", cursor.ToString(), "MATCH", pattern, "COUNT", "20");
        //            var innerResult = (RedisResult[])result;

        //            cursor = long.Parse((string)innerResult[0]);

        //            var keys = (string[])innerResult[1];

        //            foreach (var key in keys)
        //            {
        //                keysList.Add(key);
        //            }
        //        } while (cursor != 0);

        //        List<News> newsList = new List<News>();

        //        foreach (var key in keysList)
        //        {
        //            var news = db.StringGet(key).ToString();
        //            var newsObject = JsonConvert.DeserializeObject<News>(news);
        //            newsList.Add(newsObject);
        //        }

        //        newsList.Sort(delegate (News n2, News n1) { return (n1.ViewsLastPeriod + n1.LikeCount / 3 - n2.ViewsLastPeriod - n2.LikeCount / 3); });
        //        db.StringSet("trending:news:", JsonConvert.SerializeObject(newsList.Take(10)), expiry: TimeSpan.FromHours(2));
        //        return Ok(newsList.Take(10));

        //    }
        //    else
        //    {
        //        var trendingList = JsonConvert.DeserializeObject<List<News>>(trending);
        //        return Ok(trendingList.Take(10));
        //    }
        //}




        //NE KORISTI SE
        //[Authorize]
        [HttpGet("news/ClickNews/{id}")]
        public async Task<ActionResult> ClickNewsId([FromRoute] long id)
        {
            return BadRequest("DEPRECATED");
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
                newsNeoObject.ViewsLastPeriod = (int)Math.Round(falloff * newsNeoObject.ViewsLastPeriod, 0);

                //na net pise da ne moze transaction ako imaju razlicit ttl :(
                db.StringSet($"news:{id}",
                    System.Text.Json.JsonSerializer.Serialize(newsNeoObject),
                    expiry: TimeSpan.FromHours(float.Parse(_configuration.GetSection("NewsInRedisPeriodHours").Value)));

                db.StringSet($"newsExpire:{id}", "",
                    expiry: TimeSpan.FromHours(0.96 * float.Parse(_configuration.GetSection("NewsInRedisPeriodHours").Value)));

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

            News newsObject = JsonConvert.DeserializeObject<News>(news);

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
        [HttpPut("LikeNews/{id}")]
        public async Task<ActionResult> LikeNews([FromRoute] long id)
        {
            try
            {
                var db = _redis.GetDatabase();
                var query = _neo4j.Cypher
                        .Match("(n:News)")
                        .Where((News n) => n.Id == id)
                        .Return(n => n.As<News>())
                        .Limit(1);

                var news = (await _queryCache.QueryCacheNoAdd(query, $"news:{id}"));
                if (news.Count() == 0)
                {
                    return NotFound("News not found!");
                }
                news.First().LikeCount += 1;
                db.StringSet($"news:{id}", JsonConvert.SerializeObject(news), expiry: db.KeyTimeToLive($"news:{id}"));
                await _neo4j.Cypher
                    .Match("(n:News)")
                    .Where((News n) => n.Id == id)
                    .Set("n.LikeCount=$likeCount")
                    .WithParam("likeCount", news.First().LikeCount)
                    .ExecuteWithoutResultsAsync();
                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        //[Authorize]
        [HttpPut("news/DislikeNews/{userId}/{newsId}")]
        public async Task<ActionResult> DislikeNews([FromRoute] long userId, [FromRoute] long newsId)
        {
            try
            {
                var db = _redis.GetDatabase();
                await _neo4j.Cypher
                    .Match("(n:News)<-[r:LIKES]-(u:User)")
                    .Where((News n, User u) => n.Id == newsId && u.Id == userId)
                    .Delete("r")
                    .ExecuteWithoutResultsAsync();

                var queryNews = _neo4j.Cypher
                    .Match("(n:News)")
                    .Where((News n) => n.Id == newsId)
                    .Return(n => n.As<News>())
                    .Limit(1);

                var news = (await _queryCache.QueryCacheNoAdd(queryNews, $"news:{newsId}"));
                if (news.Count() == 0)
                {
                    return NotFound("News not found!");
                }

                news.First().LikeCount -= 1;
                //if (Convert.ToInt64((db.KeyTimeToLive($"news:{newsId}"))) > 0)
                //{

                db.StringSet($"news:{newsId}", JsonConvert.SerializeObject(news), expiry: db.KeyTimeToLive($"news:{newsId}"));
                //}

                await _neo4j.Cypher
                   .Match("(n:News)")
                   .Where((News n) => n.Id == newsId)
                   .Set("n.LikeCount=$likeCount")
                   .WithParam("likeCount", news.First().LikeCount)
                   .ExecuteWithoutResultsAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            

        }

        //[Authorize]
        [HttpPut("news/LikeNewsAndSetLikedRelation/{userId}/{newsId}")]
        public async Task<ActionResult> LikeNewsAndSetLikedRelation([FromRoute] long userId, [FromRoute] long newsId)
        {

            try
            {
                var db = _redis.GetDatabase();

                var existsList = (await _neo4j.Cypher
                    .Match("(n:News)<-[:LIKES]-(u:User)")
                    .Where((News n, User u) => n.Id == newsId && u.Id == userId)
                    .Return((n, u) => new
                    {
                        newsId = n.As<News>().Id,
                        userId = u.As<User>().Id
                    }).ResultsAsync).ToList();
                
                if (existsList.Any())
                {
                    return Ok("News is already liked by user");
                }


                var queryNews = _neo4j.Cypher
                        .Match("(n:News)")
                        .Where((News n) => n.Id == newsId)
                        .Return(n => n.As<News>())
                        .Limit(1);

                var news = (await _queryCache.QueryCacheNoAdd(queryNews, $"news:{newsId}"));
                if (news.Count() == 0)
                {
                    return NotFound("News not found!");
                }

                news.First().LikeCount += 1;
                //if (Convert.ToInt64((db.KeyTimeToLive($"news:{newsId}"))) > 0)
                //{

                db.StringSet($"news:{newsId}", JsonConvert.SerializeObject(news), expiry: db.KeyTimeToLive($"news:{newsId}"));
                //}

                await _neo4j.Cypher
                    .Match("(n:News), (u:User)")
                    .Where((News n, User u) => n.Id == newsId && u.Id == userId)
                    .Create("(u)-[:LIKES {date:$time}]->(n)")
                    .WithParam("time", DateTime.Now)
                    .ExecuteWithoutResultsAsync();

                await _neo4j.Cypher
                    .Match("(n:News)")
                    .Where((News n) => n.Id == newsId)
                    .Set("n.LikeCount=$likeCount")
                    .WithParam("likeCount", news.First().LikeCount)
                    .ExecuteWithoutResultsAsync();
                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        //[Authorize]
        [HttpGet("news/GetForYou")]
        public async Task<ActionResult>GetForYou()
        {
            var claims = HttpContext.User.Claims;

            var userId = Int32.Parse(claims.Where(c => c.Type == "Id").FirstOrDefault()?.Value ?? "-1");
            userId = 6;

            if (userId == -1)
                return Unauthorized("Error user not signed in");


            //var res = await _neo4j.Cypher   
            //    .Match("(u:User)-[ft:FOLLOWS_TAG]->(t:Tag)<-[tg:TAGGED]-(n:News)")
            //    .Where((User u) => u.Id == userId)
            //    .Return((n, ft) => new
            //    {
            //        News = n.As<News>(),
            //        Value=ft.As<FollowsTag>().InterestCoefficient
            //    })
            //    .ResultsAsync;

            //.Match("(u:User)-[p:PATH*0..5]-(n:News)")

            Dictionary<double, double> newsIdHash = new Dictionary<double, double>();

            var newsIdsByTags = await _neo4j.Cypher
                .Match("(u:User)-[ft:FOLLOWS_TAG]->(t:Tag)<-[tg:TAGGED]-(n:News)")
                .Where((User u) => u.Id == userId)
                .AndWhere("NOT (u)-[:SEEN]->(n)")
                .With("n, SUM(ft.InterestCoefficient) AS num")
                //.OptionalMatch("(u)-[:SEEN]->(n)")
                //.Where("NOT EXISTS((u)-[:SEEN]->(n))")
                .Return((n, num) => new
                {
                    Interest = num.As<double>(),
                    NewsId = n.As<News>().Id
                })
                .ResultsAsync;

            newsIdsByTags.ToList().ForEach(n => newsIdHash.Add(n.NewsId, n.Interest));

            var newsIdsByLocations = await _neo4j.Cypher
              .Match("(u:User)-[fl:FOLLOWS_LOCATION]->(l:Location)<-[lc:LOCATED]-(n:News)")
              .Where((User u) => u.Id == userId)
                .AndWhere("NOT (u)-[:SEEN]->(n)")
              .With("n, 4*COUNT(*) AS num")
              //.OptionalMatch("(u)-[:SEEN]->(n)")
                //.Where("NOT EXISTS((u)-[:SEEN]->(n))")
              .Return((n, num) => new
              {
                  Interest = num.As<double>(),
                  NewsId = n.As<News>().Id
              })
              .ResultsAsync;

            newsIdsByLocations.ToList().ForEach(n =>
            {
                double oldValue=0;
                newsIdHash.TryGetValue(n.NewsId, out oldValue);
                newsIdHash[n.NewsId]= oldValue + n.Interest;
            });

            var newsIdsByAuthors = await _neo4j.Cypher
              .Match("(u1:User)-[st:SUBSCRIBED_TO]->(u2:User)<-[w:WRITTEN]-(n:News)")
              .Where((User u1) => u1.Id == userId)
                .AndWhere("NOT (u1)-[:SEEN]->(n)")
              .With("n, 3*COUNT(*) AS num")
              //.OptionalMatch("(u1)-[:SEEN]->(n)")
                //.Where("NOT EXISTS((u1)-[:SEEN]->(n))")
              .Return((n, num) => new
              {
                  Interest = num.As<double>(),
                  NewsId = n.As<News>().Id
              })
              .ResultsAsync;
            newsIdsByAuthors.ToList().ForEach(n =>
            {
                double oldValue = 0;
                newsIdHash.TryGetValue(n.NewsId, out oldValue);
                newsIdHash[n.NewsId]=oldValue + n.Interest;
            });


            var mightAlsoLike = await _neo4j.Cypher
            .Match("(u:User)-[*3..5]-(n:News)")
            .Where((User u) => u.Id == userId)
            .AndWhere("shortestPath((u)-[*]-(n)) IS NOT NULL AND length(shortestPath((u)-[*]-(n))) >= 3 AND length(shortestPath((u)-[*]-(n))) <= 5")
            .ReturnDistinct(n => n.As<News>().Id)
            .ResultsAsync;

            mightAlsoLike.ToList().ForEach(n =>
            {
                double oldValue = 0;
                newsIdHash.TryGetValue(n, out oldValue);
                newsIdHash[n] = oldValue + 1;
            });
            var db = _redis.GetDatabase();

            List<News> forYou = new List<News>();
            foreach(var enrty in newsIdHash)
            {
                var query = _neo4j.Cypher
                    .Match("(n:News)")
                    .Where((News n) => n.Id == enrty.Key)
                    .Return(n => n.As<News>())
                    .Limit(1);
            var news = (await _queryCache.QueryCacheNoAdd(query, $"news:{enrty.Key}"));
                forYou.Add(news.First());
            }

            return Ok(forYou);
        }


        //[Authorize]
        [HttpPost("news/SeenNews")]
        public async Task<ActionResult> SeenNews([FromQuery] double userId, [FromQuery] double newsId)
        {
            var curDate = DateTime.Now;
            try
            {
                await _neo4j.Cypher
                            .Match("(u:User)", "(n:News)")
                            .Where((User u) => u.Id == userId)
                            .AndWhere((News n) => n.Id == newsId)
                            .Create("(u)-[s:SEEN]->(n)")
                            .Set("s.DateSeen=$dateSeen")
                            .WithParam("dateSeen", curDate)
                            .ExecuteWithoutResultsAsync();
            }catch(Exception e)
            {
                return BadRequest("Something went wrong, try again later");
            }
            return Ok();
        }
    }
}
