using Neo4jClient;
using NewsFlowAPI.Models;
using StackExchange.Redis;

namespace NewsFlowAPI.Services
{
    public class RedisNewestSubscriber : IRedisNewestSubscriber
    {
        public static bool isSubscribed = false;
        private IConnectionMultiplexer _redis;
        private IBoltGraphClient _neo4j;
        //private ISubscriber subscriber;
        private readonly string _channelForNewestNews = "newest:channel";
        private readonly string _newestNewsKey = "newestnews";

        public RedisNewestSubscriber(IConnectionMultiplexer redis, IBoltGraphClient neo4j)
        {
            _redis = redis;
            _neo4j = neo4j;
            //this.subscriber = subscriber;

        }

        public void SubscribeToSmallApi()
        {
            if (!isSubscribed)
            {
                var pubsub = _redis.GetSubscriber();

                pubsub.Subscribe(_channelForNewestNews, (channel, message) =>
                {
                    //insert in newest
                    var db = _redis.GetDatabase();

                    //if max length of new news is here, then take out the last one 
                    if (db.ListLength(_newestNewsKey) > 20)
                    {
                        db.ListRightPop(_newestNewsKey);
                    }
                    var id = Int64.Parse(message);
                    db.ListLeftPush(_newestNewsKey, message);
                    isSubscribed = true;
                });
            }

        }
    }
}
