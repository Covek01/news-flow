using Neo4jClient.Cypher;

namespace NewsFlowAPI.Services
{
    public interface IQueryCacheService
    {
        Task<IEnumerable<T>> QueryCache<T>(ICypherFluentQuery<T> neo4jQuery, string redisKey, TimeSpan? expiry = null);

        Task<IEnumerable<T>> QueryCacheParallerl<T>(ICypherFluentQuery<T> neo4jQuery, string redisKey, TimeSpan? expiry = null);
    }
}
