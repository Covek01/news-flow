using Neo4jClient;
using StackExchange.Redis;

namespace NewsFlowAPI.Services
{
    public interface IRedisNewsSubscriber
    {
        public void Subscribe(IConnectionMultiplexer redis,IBoltGraphClient neo4j);
        public void AddKey(string key);
    }
}
