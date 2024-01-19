using Microsoft.VisualBasic;
using Neo4jClient;
using NewsFlowAPI.Models;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NewsFlowAPI.Services
{
    public class RedisTrendingSubscriber : IRedisNewsSubscriber
    {
        private bool subscribed = false;
        private IConnectionMultiplexer _redis;
        private IBoltGraphClient _neo4j;
        private ISubscriber subscriber0;

        private HashSet<string> subscribedKeys=new HashSet<string>();
        public void AddKey(string key)
        {
            subscribedKeys.Add(key);
        }

        public void Subscribe(IConnectionMultiplexer redis, IBoltGraphClient neo4j)
        {
            if (!subscribed)
            {
                _redis = redis;
                var db = _redis.GetDatabase();
                db.Execute("CONFIG", "SET", "notify-keyspace-events", "Ex");
                _neo4j = neo4j;
                subscribed = true;
                subscriber0 = _redis.GetSubscriber();
                subscriber0.Subscribe("__keyevent@*__:expired", (channel, key) =>
                {
                    HandleExpiration(channel, key);
                });
            }
        }
        private async void HandleExpiration(RedisChannel channel, RedisValue key)
        {
            if (key.ToString() == "trending:news:update")
            {
                var db = _redis.GetDatabase();
                var idsTrendingListRedis = db.SortedSetRangeByRankAsync("trending:news:").Result.ToList();
                List<double> trendingList = new List<double>();
                foreach (var k in idsTrendingListRedis)
                {
                    trendingList.Add(Int32.Parse(k));
                }

                string pattern = "news:*";
                var keysList = new List<double>();
                var cursor = default(long);

                do
                {
                    var result = db.Execute("SCAN", cursor.ToString(), "MATCH", pattern, "COUNT", "20");
                    var innerResult = (RedisResult[])result;

                    cursor = long.Parse((string)innerResult[0]);
                    var keys = (string[])innerResult[1];

                    foreach (var k in keys)
                    {
                        keysList.Add(Int32.Parse(k.Substring("news:".Length)));
                    }
                } while (cursor != 0);

                var firtTimeIds = keysList.Except(trendingList).ToList();
                foreach (var id in idsTrendingListRedis)
                {
                    var news = db.StringGet($"news:{id}");
                    if (string.IsNullOrEmpty(news))
                    {
                        await db.SortedSetRemoveAsync("trending:news:", id);
                    }
                    else
                    {
                        var newsObject = JsonConvert.DeserializeObject<List<News>>(news).First();
                        //newsObject.ViewsLastPeriod = newsObject.ViewsCount;
                        //await db.SortedSetIncrementAsync("trending:news:", id, newsObject.ViewsCount-2*Int32.Parse(db.SortedSetScoreAsync("trending:news:",id).Result.ToString()));
                        await db.SortedSetAddAsync("trending:news:", id, newsObject.ViewsCount - newsObject.ViewsLastPeriod);
                        newsObject.ViewsLastPeriod = newsObject.ViewsCount;
                        List<News> newsList = new List<News>();
                        newsList.Add(newsObject);
                        db.StringSet($"news:{newsObject.Id}", JsonConvert.SerializeObject(newsList),expiry:TimeSpan.FromHours(2));
                        db.StringSet($"shadow:news:{id}", "shadow", expiry: TimeSpan.FromHours(0.95 * 2));
                    }
                }
                await db.SortedSetRemoveRangeByScoreAsync("trending:news:", Int32.MinValue, 0);
                foreach (var id in firtTimeIds)
                {
                    await db.SortedSetAddAsync("trending:news:", id, 0);
                }
                db.StringSet("trending:news:update", "trending_expiration", TimeSpan.FromHours(0.5));
            }
        }
    }
}
