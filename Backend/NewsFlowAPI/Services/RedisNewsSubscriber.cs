using Microsoft.AspNetCore.Connections;
using Neo4jClient;
using NewsFlowAPI.Models;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NewsFlowAPI.Services
{
    public class RedisNewsSubscriber : IRedisNewsSubscriber
    {
        private bool subscribed = false;
        private  IConnectionMultiplexer _redis;
        private IBoltGraphClient _neo4j;
        private ISubscriber subscriber0;

        //Dictionary<string, string> subscribedKeys = new Dictionary<string, string>();         
        private HashSet<string> subscribedKeys = new HashSet<string>(); 
        public void AddKey(string key)
        {
            subscribedKeys.Add(key);
        }

        public void Subscribe(IConnectionMultiplexer redis,IBoltGraphClient neo4j)
        {
            if(!subscribed)
            {
                _redis = redis;
                var db = _redis.GetDatabase();
                db.Execute("CONFIG" ,"SET", "notify-keyspace-events","Ex");
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
            string keyString=key.ToString();
            if (this.subscribedKeys.Contains(keyString))
            {
                long id = Int32.Parse(keyString.Substring(11));
                var db = _redis.GetDatabase();
                var news = db.StringGet($"news:{id}").ToString();
                News newsObject = JsonConvert.DeserializeObject<News>(news);
                
                if (newsObject != null)
                {
                    var date = DateTime.Now;
                    await db.KeyDeleteAsync($"news:{id}");
                    await _neo4j.Cypher
                        .Match("(n:News)")
                        .Where((News n) => n.Id == id)
                        .Set("n.ViewsLastPeriod=$views, n.LastPeriodTime=$last")
                        .WithParams(new {views=newsObject.ViewsLastPeriod, last=date})
                        //.WithParam("views", newsObject.ViewsLastPeriod)
                        .ExecuteWithoutResultsAsync();
                }
            }
        }


    }
}
